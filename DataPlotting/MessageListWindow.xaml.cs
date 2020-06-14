using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Message = DataLoader.Model.Message;

namespace DataPlotting
{
    public partial class MessageListWindow : Window
    {
        public List<Message> Messages { get; set; }
        public MessageListWindow(List<Message> messages)
        {
            Messages = messages;
            InitializeComponent();
            Messages = Messages.OrderByDescending(x => x.Time).ToList();
            MessagesDataGrid.DataContext = Messages;
        }
    }
}
