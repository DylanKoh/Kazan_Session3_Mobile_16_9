using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Kazan_Session3_Mobile_16_9.GlobalClass;

namespace Kazan_Session3_Mobile_16_9
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisteringNewPM : ContentPage
    {
        List<Asset> _assetList;
        List<Task1> _taskList;
        List<AssetOdometer> _odometerList;
        List<PMScheduleModel> _modelList;
        List<AssetList> _toAddAsset = new List<AssetList>();
        public RegisteringNewPM()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await LoadPickers();
        }

        private async Task LoadPickers()
        {
            pAssetName.Items.Clear();
            pTask.Items.Clear();
            pScheduleModel.Items.Clear();
            var client = new WebApi();
            var assetResponse = await client.PostAsync(null, "Assets");
            _assetList = JsonConvert.DeserializeObject<List<Asset>>(assetResponse);

            var taskResponse = await client.PostAsync(null, "Tasks");
            _taskList = JsonConvert.DeserializeObject<List<Task1>>(taskResponse);
            foreach (var item in _taskList)
            {
                pTask.Items.Add(item.Name);
            }

            var odometerResponse = await client.PostAsync(null, "AssetOdometers");
            _odometerList = JsonConvert.DeserializeObject<List<AssetOdometer>>(odometerResponse);

            var modelResponse = await client.PostAsync(null, "PMScheduleModels");
            _modelList = JsonConvert.DeserializeObject<List<PMScheduleModel>>(modelResponse);
        }

        private void pTask_SelectedIndexChanged(object sender, EventArgs e)
        {
            pAssetName.Items.Clear();
            pScheduleModel.ItemsSource = null;
            lvAssets.ItemsSource = null;
            _toAddAsset.Clear();
            if (pTask.SelectedIndex >= 0 && pTask.SelectedIndex <= 3)
            {
                var relevantAssets = (from x in _odometerList
                                      join y in _assetList on x.AssetID equals y.ID
                                      select y.AssetName).Distinct().ToList();
                foreach (var item in relevantAssets)
                {
                    pAssetName.Items.Add(item);
                }
                var getRelevantModel = (from x in _modelList
                                        where x.PMScheduleTypeID == 1
                                        select x.Name);
                pScheduleModel.ItemsSource = getRelevantModel.ToList();

            }
            else 
            {
                var nonRelevantAssets = (from x in _odometerList
                                         select x.AssetID).Distinct();
                var getRelevantAssets = (from x in _assetList
                                         where !nonRelevantAssets.Contains(x.ID)
                                         select x.AssetName).Distinct().ToList();
                foreach (var item in getRelevantAssets)
                {
                    pAssetName.Items.Add(item);
                }
                var getRelevantModel = (from x in _modelList
                                        where x.PMScheduleTypeID == 2
                                        select x.Name);
                pScheduleModel.ItemsSource = getRelevantModel.ToList();
            }
            
        }

        private void btnAdd_Clicked(object sender, EventArgs e)
        {
            if (pAssetName.SelectedItem != null)
            {
                lvAssets.ItemsSource = null;
                _toAddAsset.Add((from x in _assetList
                               where x.AssetName == pAssetName.SelectedItem.ToString()
                               select new AssetList() { AssetName =  x.AssetName,  AssetID = x.ID}).FirstOrDefault());
                lvAssets.ItemsSource = _toAddAsset;
            }
            
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var parentLayout = (StackLayout)button.Parent;
            var lblassetID = ((Label)parentLayout.Children[0]).Text;
            var assetID = long.Parse(lblassetID);
            lvAssets.ItemsSource = null;
            _toAddAsset.Remove((from x in _toAddAsset
                                where x.AssetID == assetID
                                select x).FirstOrDefault());
            lvAssets.ItemsSource = _toAddAsset;
        }

        private void pScheduleModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pScheduleModel.SelectedItem != null && pScheduleModel.SelectedItem.ToString() == "Every X Kilometer")
            {
                entryReminder.IsVisible = true;
                entryStart.IsVisible = true;
                entryEnd.IsVisible = true;
                dpStart.IsVisible = false;
                dpEnd.IsVisible = false;
            }
            else if (pScheduleModel.SelectedItem != null && pScheduleModel.SelectedItem.ToString() != "Every X Kilometer")
            {
                entryReminder.IsVisible = false;
                entryStart.IsVisible = false;
                entryEnd.IsVisible = false;
                dpStart.IsVisible = true;
                dpEnd.IsVisible = true;
            }
            else if (pScheduleModel.SelectedItem == null)
            {
                entryReminder.IsVisible = false;
                entryStart.IsVisible = false;
                entryEnd.IsVisible = false;
                dpStart.IsVisible = false;
                dpEnd.IsVisible = false;
            }
        }

        private async void btnSubmit_Clicked(object sender, EventArgs e)
        {
            if (pScheduleModel.SelectedItem == null)
            {
                await DisplayAlert("Submit", "Please select a schedule model!", "Ok");
            }
            else
            {
                var getTaskID = (from x in _taskList
                                 where x.Name == pTask.SelectedItem.ToString()
                                 select x.ID).FirstOrDefault();
                var client = new WebApi();
                var boolCheck = true;
                if (pScheduleModel.SelectedItem.ToString() == "Every X Kilometer")
                {
                    var internalCheck = true;
                    foreach (var item in _toAddAsset)
                    {
                        var getInterval = long.Parse(entryEnd.Text) - long.Parse(entryStart.Text);
                        var getNumberOfReminders = Convert.ToInt32(getInterval / long.Parse(entryReminder.Text));
                        var numberAddition = 1;
                        for (int i = 0; i < getNumberOfReminders; i++)
                        {
                            var newPMTask = new PMTask()
                            {
                                AssetID = item.AssetID,
                                PMScheduleTypeID = 1,
                                ScheduleKilometer = (numberAddition * long.Parse(entryReminder.Text)) + long.Parse(entryEnd.Text),
                                TaskDone = false,
                                TaskID = getTaskID
                            };
                            var response = await client.PostAsync(JsonConvert.SerializeObject(newPMTask), "PMTasks/Create");
                            if (response == "\"Unable to add same task within similar range of values!\"" || response != "\"Created PM Task successfully!\"")
                            {
                                boolCheck = false;
                                internalCheck = false;
                            }
                            if (internalCheck == false)
                                break;
                            numberAddition += 1;
                        }
                        if (internalCheck == false)
                            break;
                    }
                    if (internalCheck == false)
                    {
                        await DisplayAlert("Submit", "Please change your range to a higher value than already completed or ongoing task!", "Ok");
                    }
                }
                else if (pScheduleModel.SelectedItem.ToString() == "Daily")
                {
                    foreach (var item in _toAddAsset)
                    {
                        for (var i = dpStart.Date; i <= dpEnd.Date; i = i.AddDays(1))
                        {
                            var newPMTask = new PMTask()
                            {
                                AssetID = item.AssetID,
                                PMScheduleTypeID = 2,
                                ScheduleDate = i,
                                TaskDone = false,
                                TaskID = getTaskID
                            };
                            var response = await client.PostAsync(JsonConvert.SerializeObject(newPMTask), "PMTasks/Create");
                            if (response != "\"Created PM Task successfully!\"")
                            {
                                boolCheck = false;
                            }
                        }
                    }
                }
                else if (pScheduleModel.SelectedItem.ToString() == "Weekly")
                {
                    foreach (var item in _toAddAsset)
                    {
                        for (var i = dpStart.Date; i <= dpEnd.Date; i = i.AddDays(7))
                        {
                            var newPMTask = new PMTask()
                            {
                                AssetID = item.AssetID,
                                PMScheduleTypeID = 2,
                                ScheduleDate = i,
                                TaskDone = false,
                                TaskID = getTaskID
                            };
                            var response = await client.PostAsync(JsonConvert.SerializeObject(newPMTask), "PMTasks/Create");
                            if (response != "\"Created PM Task successfully!\"")
                            {
                                boolCheck = false;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in _toAddAsset)
                    {
                        for (var i = dpStart.Date; i <= dpEnd.Date; i = i.AddMonths(1))
                        {
                            var newPMTask = new PMTask()
                            {
                                AssetID = item.AssetID,
                                PMScheduleTypeID = 2,
                                ScheduleDate = i,
                                TaskDone = false,
                                TaskID = getTaskID
                            };
                            var response = await client.PostAsync(JsonConvert.SerializeObject(newPMTask), "PMTasks/Create");
                            if (response != "\"Created PM Task successfully!\"")
                            {
                                boolCheck = false;
                            }
                        }
                    }
                }
                if (boolCheck == false)
                {
                    await DisplayAlert("Submit", "An error occured while creating new tasks! Please contact our administrator!", "Ok");

                }
                else
                {
                    await DisplayAlert("Submit", "Successfully created PM Task(s)!", "Ok");
                    await Navigation.PopAsync();
                }
            }
            
        }

        private async void btnCancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}