using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Kazan_Session3_Mobile_16_9.GlobalClass;

namespace Kazan_Session3_Mobile_16_9
{
    public partial class MainPage : ContentPage
    {
        List<Asset> _assetList;
        List<Task1> _taskList;
        List<CustomView> _customList = new List<CustomView>();
        public MainPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await LoadPickers();
            await LoadData(dpActive.Date);
        }

        private async Task LoadData(DateTime activeDate)
        {
            lvPMTask.ItemsSource = null;
            _customList.Clear();
            var client = new WebApi();
            var runResponse = await client.PostAsync(null, "PMTasks/GetRunTaskNotDone");
            _customList = JsonConvert.DeserializeObject<List<CustomView>>(runResponse);
            var timeOverResponse = await client.PostAsync(null, $"PMTasks/GetOverTime?activeDate={activeDate}");
            _customList.AddRange(JsonConvert.DeserializeObject<List<CustomView>>(timeOverResponse));
            var timeCurrentResponse = await client.PostAsync(null, $"PMTasks/GetCurrentTime?activeDate={activeDate}");
            _customList.AddRange(JsonConvert.DeserializeObject<List<CustomView>>(timeCurrentResponse));
            var timeAboutResponse = await client.PostAsync(null, $"PMTasks/GetAboutTime?activeDate={activeDate}");
            _customList.AddRange(JsonConvert.DeserializeObject<List<CustomView>>(timeAboutResponse));
            var runDoneResponse = await client.PostAsync(null, $"PMTasks/GetRunTaskDone");
            _customList.AddRange(JsonConvert.DeserializeObject<List<CustomView>>(runDoneResponse));
            var timeDoneResponse = await client.PostAsync(null, $"PMTasks/GetTimeDone");
            _customList.AddRange(JsonConvert.DeserializeObject<List<CustomView>>(timeDoneResponse));
            lvPMTask.ItemsSource = _customList;
        }

        private async Task LoadPickers()
        {
            pAssetName.Items.Clear();
            pTask.Items.Clear();
            var client = new WebApi();
            var assetResponse = await client.PostAsync(null, "Assets");
            _assetList = JsonConvert.DeserializeObject<List<Asset>>(assetResponse);
            foreach (var item in _assetList)
            {
                pAssetName.Items.Add(item.AssetName);
            }

            var taskResponse = await client.PostAsync(null, "Tasks");
            _taskList = JsonConvert.DeserializeObject<List<Task1>>(taskResponse);
            foreach (var item in _taskList)
            {
                pTask.Items.Add(item.Name);
            }

        }

        private async void dpActive_DateSelected(object sender, DateChangedEventArgs e)
        {
            await LoadData(dpActive.Date);
        }

        private async void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var client = new WebApi();
            var checkBox = (CheckBox)sender;
            var parentLayout = (StackLayout)checkBox.Parent;
            var childToTake = (StackLayout)parentLayout.Children[0];
            var PMIDLabel = ((Label)childToTake.Children[0]).Text;
            var response = "";

            if (e.Value == true)
            {
                response = await client.PostAsync(null, $"PMTasks/UpdateDone?PMID={PMIDLabel}&changedState={e.Value}");
            }
            else
            {
                response = await client.PostAsync(null, $"PMTasks/UpdateNotDone?PMID={PMIDLabel}&changedState={e.Value}");
            }
            if (response == "\"Completed Update!\"")
            {
                await DisplayAlert("Update PM Task", "Completed Update!", "Ok");
                await LoadData(dpActive.Date);
            }





        }

        private async void pAssetName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pAssetName.SelectedItem != null)
            {
                FilterList();
            }
            else
            {
                await LoadData(dpActive.Date);
            }
        }

        private void FilterList()
        {
            if (pAssetName.SelectedItem != null && pTask.SelectedItem != null)
            {
                var filteredList = (from x in _customList
                                    where x.Asset.Contains(pAssetName.SelectedItem.ToString()) && x.TaskName.Contains(pTask.SelectedItem.ToString())
                                    select x).ToList();
                lvPMTask.ItemsSource = filteredList;
            }
            else if (pAssetName.SelectedItem == null && pTask.SelectedItem != null)
            {
                var filteredList = (from x in _customList
                                    where x.TaskName.Contains(pTask.SelectedItem.ToString())
                                    select x).ToList();
                lvPMTask.ItemsSource = filteredList;
            }
            else if (pAssetName.SelectedItem != null && pTask.SelectedItem == null)
            {
                var filteredList = (from x in _customList
                                    where x.Asset.Contains(pAssetName.SelectedItem.ToString())
                                    select x).ToList();
                lvPMTask.ItemsSource = filteredList;
            }
        }

        private async void pTask_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pTask.SelectedItem != null)
            {
                FilterList();
            }
            else
            {
                await LoadData(dpActive.Date);
            }
        }

        private async void btnClear_Clicked(object sender, EventArgs e)
        {
            await LoadPickers();
        }

        private void btnAdd_Clicked(object sender, EventArgs e)
        {

        }

    }
}

