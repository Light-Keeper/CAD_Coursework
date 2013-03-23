using MediatorLib;
using WPF_GUI.Helpers;

namespace WPF_GUI.ViewModels
{
    public class BaseViewModel : NotificationObject
    {
        public readonly static Mediator Mediator = new Mediator();
    }
}
