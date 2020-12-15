using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using WeatherApp.Commands;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region Membres

        private BaseViewModel currentViewModel;
        private List<BaseViewModel> viewModels;
        private TemperatureViewModel tvm;
        private OpenWeatherService ows;
        private string filename;

        private VistaSaveFileDialog saveFileDialog;
        private VistaOpenFileDialog openFileDialog;
        public DelegateCommand<string> OpenFileDialogCommand { get; set; }
        public DelegateCommand<string> SaveFileDialogCommand { get; set; }
        public DelegateCommand<string> ChangeLanguageCommand { get; set; }

        #endregion

        #region Propriétés
        /// <summary>
        /// Model actuellement affiché
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set { 
                currentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// String contenant le nom du fichier
        /// </summary>
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }

        /// <summary>
        /// Commande pour changer la page à afficher
        /// </summary>
        public DelegateCommand<string> ChangePageCommand { get; set; }

        /// <summary>
        /// TODO 02 : Ajouter ImportCommand
        /// </summary>

        /// <summary>
        /// TODO 02 : Ajouter ExportCommand
        /// </summary>

        /// <summary>
        /// TODO 13a : Ajouter ChangeLanguageCommand
        /// </summary>
        private string saveFilename;

        public string SaveFilename
        {
            get { return saveFilename; }
            set
            {
                saveFilename = value;
                OnPropertyChanged();
            }
        }

        private string openFilename;

        public string OpenFilename
        {
            get { return openFilename; }
            set
            {
                openFilename = value;
                OnPropertyChanged();
            }
        }

        private string fileContent;

        public string FileContent
        {
            get { return fileContent; }
            set
            {
                fileContent = value;
                OnPropertyChanged();
            }
        }

        public List<BaseViewModel> ViewModels
        {
            get {
                if (viewModels == null)
                    viewModels = new List<BaseViewModel>();
                return viewModels; 
            }
        }
        #endregion

        public ApplicationViewModel()
        {
            ChangePageCommand = new DelegateCommand<string>(ChangePage);

            /// TODO 06 : Instancier ExportCommand qui doit appeler la méthode Export
            /// Ne peut s'exécuter que la méthode CanExport retourne vrai

            /// TODO 03 : Instancier ImportCommand qui doit appeler la méthode Import

            /// TODO 13b : Instancier ChangeLanguageCommand qui doit appeler la méthode ChangeLanguage
            ChangePageCommand = new DelegateCommand<string>(ChangePage);
            OpenFileDialogCommand = new DelegateCommand<string>(Import);
            SaveFileDialogCommand = new DelegateCommand<string>(Export, CanExport);
            ChangeLanguageCommand = new DelegateCommand<string>(ChangeLanguage);
            initViewModels();          

            CurrentViewModel = ViewModels[0];

        }

        #region Méthodes
        void initViewModels()
        {
            /// TemperatureViewModel setup
            tvm = new TemperatureViewModel();

            string apiKey = "";

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "DEVELOPMENT")
            {
                apiKey = AppConfiguration.GetValue("OWApiKey");
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.apiKey) && apiKey == "")
            {
                tvm.RawText = "Aucune clé API, veuillez la configurer";
            } else
            {
                if (apiKey == "")
                    apiKey = Properties.Settings.Default.apiKey;

                ows = new OpenWeatherService(apiKey);
            }
                
            tvm.SetTemperatureService(ows);
            ViewModels.Add(tvm);

            var cvm = new ConfigurationViewModel();
            ViewModels.Add(cvm);
        }



        private void ChangePage(string pageName)
        {            
            if (CurrentViewModel is ConfigurationViewModel)
            {
                ows.SetApiKey(Properties.Settings.Default.apiKey);

                var vm = (TemperatureViewModel)ViewModels.FirstOrDefault(x => x.Name == typeof(TemperatureViewModel).Name);
                if (vm.TemperatureService == null)
                    vm.SetTemperatureService(ows);                
            }

            CurrentViewModel = ViewModels.FirstOrDefault(x => x.Name == pageName);  
        }

        /// <summary>
        /// TODO 07 : Méthode CanExport ne retourne vrai que si la collection a du contenu
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExport(string obj)
        {
                return true;      
        }

        /// <summary>
        /// Méthode qui exécute l'exportation
        /// </summary>
        /// <param name="obj"></param>
        private void Export(string obj)
        {

            if (saveFileDialog == null)
            {
                saveFileDialog = new VistaSaveFileDialog();
                saveFileDialog.Filter = "Json file|*.json|All files|*.*";
                saveFileDialog.DefaultExt = "json";
            }
            ChooseFileToSave(obj);
            /// TODO 08 : Code pour afficher la boîte de dialogue de sauvegarde
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : demo_openFolderDialog
            /// ---
            /// Algo
            /// Si la réponse de la boîte de dialogue est vrai
            ///   Garder le nom du fichier dans Filename
            ///   Appeler la méthode saveToFile
            ///   

        }
        private void ChooseFileToSave(string obj)
        {

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFilename = saveFileDialog.FileName;
                saveToFile();
            }
        }
        private void saveToFile()
        {
            using (var tw = new StreamWriter(SaveFilename, false))
            {
                var data = tvm.Temperatures;

                var resultat = JsonConvert.SerializeObject(data, Formatting.Indented);


                tw.WriteLine(resultat);
                tw.Close();
            }
            /// TODO 09 : Code pour sauvegarder dans le fichier
            /// Voir 
            /// Solution : 14_pratique_examen
            /// Projet : serialization_object
            /// Méthode : serialize_array()
            /// 
            /// ---
            /// Algo
            /// Initilisation du StreamWriter
            /// Sérialiser la collection de températures
            /// Écrire dans le fichier
            /// Fermer le fichier           

        }

        private void openFromFile()
        {
            if (!File.Exists(OpenFilename))
            {
                Console.WriteLine($"Le fichier {OpenFilename} n'existe pas. Veuillez le générer à partir de la sérialisation d'un tableau vers un fichier.");
                Console.ReadKey();
                return;
            }

            List<TemperatureModel> data;
            using (StreamReader sr = File.OpenText(OpenFilename))
            {
                var fileContent = sr.ReadToEnd();

                data = JsonConvert.DeserializeObject<List<TemperatureModel>>(fileContent);
            }
            tvm.Temperatures.Clear();
            foreach (TemperatureModel temp in data)
            {
                tvm.Temperatures.Add(temp);
            }

            /// TODO 05 : Code pour lire le contenu du fichier
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : serialization_object
            /// Méthode : deserialize_from_file_to_object
            /// 
            /// ---
            /// Algo
            /// Initilisation du StreamReader
            /// Lire le contenu du fichier
            /// Désérialiser dans un liste de TemperatureModel
            /// Remplacer le contenu de la collection de Temperatures avec la nouvelle liste

        }

        private void Import(string obj)
        {
            if (openFileDialog == null)
            {
                openFileDialog = new VistaOpenFileDialog();
                openFileDialog.Filter = "Json file|*.json|All files|*.*";
                openFileDialog.DefaultExt = "json";
            }
            SelectFile(obj);

            /// TODO 04 : Commande d'importation : Code pour afficher la boîte de dialogue


        }
        private void SelectFile(string obj)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                OpenFilename = openFileDialog.FileName;
                openFromFile();
            }
        }
        public void Restart()
        {

            var filename = Application.ResourceAssembly.Location;
            var newFile = Path.ChangeExtension(filename, ".exe");
            Process.Start(newFile);
            Application.Current.Shutdown();
        }
        private void ChangeLanguage(string language)
        {
            Properties.Settings.Default.Language = language;
            Properties.Settings.Default.Save();

            if (MessageBox.Show(
                    "Restart lapplication",
                    "Warning",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Restart();

        }

        #endregion
    }
}
