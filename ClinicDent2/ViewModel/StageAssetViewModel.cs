using ClinicDent2.Commands;
using ClinicDent2.Model;
using System;
using System.Windows;

namespace ClinicDent2.ViewModel
{
    public class StageAssetViewModel:BaseViewModel
    {
        public StageAsset stageAsset;
        public RelayCommand PostToServerCommand { get; set; }
        private void PostToServer(object arg)
        {
            try
            {
                HttpService.PostStageAsset(stageAsset);
                switch (stageAsset.Type)
                {
                    case AssetType.Bond:
                        StageViewModel.Bonds.Add(stageAsset);
                        break;
                    case AssetType.Dentin:
                        StageViewModel.Dentins.Add(stageAsset);
                        break;
                    case AssetType.Enamel:
                        StageViewModel.Enamels.Add(stageAsset);
                        break;
                    case AssetType.CanalMethod:
                        StageViewModel.CanalMethods.Add(stageAsset);
                        break;
                    case AssetType.Sealer:
                        StageViewModel.Sealers.Add(stageAsset);
                        break;
                    case AssetType.Cement:
                        StageViewModel.Cements.Add(stageAsset);
                        break;
                    case AssetType.Technician:
                        StageViewModel.Technicians.Add(stageAsset);
                        break;
                    case AssetType.Pin:
                        StageViewModel.Pins.Add(stageAsset);
                        break;
                    case AssetType.Operation:
                        StageViewModel.Operations.Add(stageAsset);
                        break;
                    case AssetType.Calcium:
                        StageViewModel.Calciums.Add(stageAsset);
                        break;
                }
                MessageBox.Show("Додано");


                stageAsset = new StageAsset();
                NotifyPropertyChanged("Id");
                NotifyPropertyChanged("Type");
                NotifyPropertyChanged("Value");
            }
            catch(Exception e)
            {
                MessageBox.Show($"Не вдалось створити параметр: {e.Message}", "Помилка");
            }
        }
        public StageAssetViewModel(StageAsset stageAssetToSet)
        {
            stageAsset = stageAssetToSet;
            PostToServerCommand = new RelayCommand(PostToServer);
        }
        public int Id
        {
            get
            {
                return stageAsset.Id;
            }
            set
            {
                if(stageAsset.Id != value)
                {
                    stageAsset.Id = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public AssetType Type
        {
            get
            {
                return stageAsset.Type;
            }
            set
            {
                if (stageAsset.Type != value)
                {
                    stageAsset.Type = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Value
        {
            get
            {
                return stageAsset.Value;
            }
            set
            {
                if (stageAsset.Value != value)
                {
                    stageAsset.Value = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
