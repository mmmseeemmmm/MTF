using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MTFApp
{
    class Warning : INotifyPropertyChanged
    {
        private static readonly Warning instance = new Warning();
        private Warning()
        { }

        public static Warning Instance => instance;

        private static ObservableCollection<WarningMessage> warningMessages = new ObservableCollection<WarningMessage>();
        public static void Add(WarningMessage warningMessage)
        {
            if (warningMessages.Any(m => m.Id == warningMessage.Id))
            {
                return;
            }

            warningMessages.Add(warningMessage);
            instance.OnPropertyChanged(nameof(Count));
        }

        public static void Remove(Guid warningMessageId)
        {
            var message = warningMessages.FirstOrDefault(m => m.Id == warningMessageId);
            if (message != null)
            {
                warningMessages.Remove(message);
                instance.OnPropertyChanged(nameof(Count));
            }
        }

        public static void Clear()
        {
            warningMessages.Clear();
            instance.OnPropertyChanged(nameof(Count));
        }

        public static int Count => warningMessages.Count;

        public static ObservableCollection<WarningMessage> Messages => warningMessages;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WarningMessage
    {
        public WarningMessage()
        {
            Id = Guid.NewGuid();
        }

        public string Message { get; set; }
        public Guid Id { get; set; }
    }
}
