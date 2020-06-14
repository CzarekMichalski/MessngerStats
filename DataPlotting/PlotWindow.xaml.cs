using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DataLoader;
using DataLoader.Utils;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using Axis = OxyPlot.Axes.Axis;
using ComboBox = System.Windows.Controls.ComboBox;
using DateTimeAxis = OxyPlot.Axes.DateTimeAxis;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace DataPlotting
{
    public partial class PlotWindow : Window
    {
        private string _selectedPlotType { get; set; }
        private string _selectedConversation { get; set; }
        private bool _fromGroup { get; set; }
        private DbManager _databaseManager { get; set; } = new DbManager();
        private TitleInfo _info;
        private UsernameWrapper _nameWrapper { get; set; }
        public List<DataPoint> Data { get; set; }
        public PlotModel Model { get; set; }
        public List<string> PlotTypes { get; set; }
        public List<string> ConversationType { get; set; }
        public bool LabelsOn { get; set; } = true;
        public List<string> ConversationList { get; set; }
        public DateTime From { get; set; } = DateTime.Today;
        public DateTime To { get; set; } = DateTime.Today;
        public string Keyword { get; set; }


        public PlotWindow()
        {
            DataContext = this;
            InitPlotTypeList();
            InitConversationTypeList();
            ConversationList = _databaseManager.GetPrivateConversations();
        }

        public void InitModel(TitleInfo info, IDictionary<DateTime, int> data, string stringFormat)
        {
            _info = info;
            Data = ConvertDictionaryToDataPoints(data);
            CreateModel(stringFormat);
            PlotView.Model = Model;
        }

        private List<DataPoint> ConvertDictionaryToDataPoints(IDictionary<DateTime, int> data)
        {
            var dataPoints = new List<DataPoint>();

            foreach (var (date, value) in data)
            {
                var point = new DataPoint(Axis.ToDouble(date), value);
                dataPoints.Add(point);
            }

            return dataPoints;
        }

        private void CreateModel(string stringFormat = null)
        {
            if (Data == null)
            {
                return;
            }

            if (Data.Count == 0)
            {
                MessageBox.Show("To few messages to plot properly");
                return;
            }

            Model = new PlotModel();
            Model.Title = _info.Title;

            var offset = 0.0001;
            var step = 30.0;

            if (stringFormat == null || stringFormat == "HH:mm")
            {
                offset = 0;
            }

            if (stringFormat == "HH:mm")
            {
                step = 1 / 24.0;
            }

            if (LabelsOn)
            {
                Model.Axes.Add(new DateTimeAxis
                {
                    Position = AxisPosition.Bottom,
                    Maximum = Data.Max(x => x.X) + offset * Data.Max(x => x.X),
                    Minimum = Data.Min(x => x.X) - offset * Data.Min(x => x.X),
                    StringFormat = stringFormat,
                    Title = _info.XAxisTitle,
                    MajorStep = step,
                    Angle = 280,
                    AxisTickToLabelDistance = 60
                });
            }
            else
            {
                Model.Axes.Add(new DateTimeAxis
                {
                    IsAxisVisible = false
                });
            }

            Model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Maximum = Data.Max(x => x.Y) + 0.1 * Data.Max(x => x.Y),
                Minimum = 0,
                Title = _info.YAxisTitle
            });

            var series = new LineSeries();
            series.Points.AddRange(Data);
            Model.Series.Add(series);
        }

        private void InitPlotTypeList()
        {
            PlotTypes = new List<string>();
            PlotTypes.Add("Cumulative sum");
            PlotTypes.Add("Hour activity");
        }

        private void InitConversationTypeList()
        {
            ConversationType = new List<string>();
            ConversationType.Add("Private");
            ConversationType.Add("Group");
        }

        private void LabelCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            var stringFormat = _selectedPlotType switch
            {
                "Cumulative sum" => "MM/dd/yyyy",
                "Hour activity" => "HH:mm",
                _ => null
            };

            LabelsOn = false;
            CreateModel(stringFormat);
            PlotView.Model = Model;
        }

        private void LabelCheckboxChecked(object sender, RoutedEventArgs e)
        {
            if (PlotView == null)
            {
                return;
            }

            var stringFormat = _selectedPlotType switch
            {
                "Cumulative sum" => "MM/dd/yyyy",
                "Hour activity" => "HH:mm",
                _ => null
            };

            LabelsOn = true;
            CreateModel(stringFormat); 
            PlotView.Model = Model;
        }

        private void ConversationTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            var conversationTypeCombobox = sender as ComboBox;

            if (conversationTypeCombobox == null)
            {
                return;
            }

            if (ConversationCombobox == null)
            {
                return;
            }

            _fromGroup = (conversationTypeCombobox.SelectedItem as string) switch
            {
                "Group" => true,
                "Private" => false
            };

            ConversationCombobox.ItemsSource = (conversationTypeCombobox.SelectedItem as string) switch
            {
                "Group" => _databaseManager.GetGroups(),
                "Private" => _databaseManager.GetPrivateConversations()
            };
        }

        private void SavePlot(object sender, RoutedEventArgs e)
        {
            if (Model == null)
            {
                return;
            }

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                var pngExporter = new PngExporter { Width = 1920, Height = 1080};
                pngExporter.ExportToFile(Model, $"{saveFileDialog.FileName}.png");
            }
        }

        private void OpenSearchWindow(object sender, RoutedEventArgs e)
        {
            var searchWindow = new SearchWindow(_databaseManager.GetPrivateConversations(), _databaseManager.GetGroups());
            searchWindow.ShowDialog();
        }

        private void GetDateRangeForConversation(object sender, SelectionChangedEventArgs e)
        {
            if (ConversationCombobox == null)
            {
                return;
            }

            _selectedConversation = ConversationCombobox.SelectedItem as string;
            var messages = _databaseManager.GetMessages(_fromGroup, _selectedConversation);

            if (messages.Count == 0)
            {
                return;    
            }

            From = messages.Min(x => x.SendTime).Date;
            To = messages.Max(x => x.SendTime).Date;
            FromDatePicker.SelectedDate = From;
            ToDatePicker.SelectedDate = To;
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void PlotButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_selectedPlotType == "Cumulative sum")
            {
                var messages = _databaseManager.GetMessages(_fromGroup, _selectedConversation, From, To,
                    keyword: SearchTextbox.Text);

                if (messages.Count < 2)
                {
                    MessageBox.Show("This conversation contains no messages");
                    return;
                }

                var cumulativeSum = MessagesToCumulativeSum.Convert(messages);

                _info = new TitleInfo
                {
                    Title = _selectedConversation,
                    XAxisTitle = "Data",
                    YAxisTitle = "Suma skumulowana"
                };

                InitModel(_info, cumulativeSum, "dd/MM/yyyy");
            }
            else if(_selectedPlotType == "Hour activity")
            {
                var messages = _databaseManager.GetMessages(_fromGroup, _selectedConversation, From, To,
                    keyword: SearchTextbox.Text);
                if (messages.Count < 2)
                {
                    MessageBox.Show("This conversation contains no messages");
                    return;
                }

                var hourActivity = MessagesToHourActivity.Convert(messages);

                _info = new TitleInfo
                {
                    Title = _selectedConversation,
                    XAxisTitle = "Godzina",
                    YAxisTitle = "Liczba wiadomości"
                };

                InitModel(_info, hourActivity, "HH:mm");
            }

            SearchTextbox.Text = "";
        }

        private void PlotTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlotTypeCombobox == null)
            {
                return;
            }

            _selectedPlotType = PlotTypeCombobox.SelectedItem as string;
        }

        private void LoadMessages(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog();

            _nameWrapper = new UsernameWrapper();
            var enterNameWindow = new EnterNameWindow(_nameWrapper);
            enterNameWindow.ShowDialog();

            if (_nameWrapper.UserName == null)
            {
                MessageBox.Show("Name can not be null");
                LoadingLabel.Visibility = Visibility.Hidden;
                return;
            }

            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadingLabel.Visibility = Visibility.Visible;
                var dbLoader = new DbManager(folderBrowser.SelectedPath, _nameWrapper.UserName);

                try
                {
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        var messageCount = dbLoader.Run();
                        MessageBox.Show($"Loaded {messageCount} messages.");
                        Dispatcher.Invoke(() => { LoadingLabel.Visibility = Visibility.Hidden; });
                    }).Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Loading error {ex}");
                }
            }
        }

        private void DeleteMessages(object sender, RoutedEventArgs e)
        {
            _databaseManager.DeleteAllMessages();
        }
    }
}
