using ClinicDent2.Commands;
using ClinicDentClientCommon.Model;
using ClinicDentClientCommon.Services;
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
                        StageAsset.Bonds.Add(stageAsset);
                        break;
                    case AssetType.Dentin:
                        StageAsset.Dentins.Add(stageAsset);
                        break;
                    case AssetType.Enamel:
                        StageAsset.Enamels.Add(stageAsset);
                        break;
                    case AssetType.CanalMethod:
                        StageAsset.CanalMethods.Add(stageAsset);
                        break;
                    case AssetType.Sealer:
                        StageAsset.Sealers.Add(stageAsset);
                        break;
                    case AssetType.Cement:
                        StageAsset.Cements.Add(stageAsset);
                        break;
                    case AssetType.Technician:
                        StageAsset.Technicians.Add(stageAsset);
                        break;
                    case AssetType.Pin:
                        StageAsset.Pins.Add(stageAsset);
                        break;
                    case AssetType.Operation:
                        StageAsset.Operations.Add(stageAsset);
                        break;
                    case AssetType.Calcium:
                        StageAsset.Calciums.Add(stageAsset);
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
