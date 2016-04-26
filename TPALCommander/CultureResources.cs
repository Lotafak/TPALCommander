using System.Globalization;
using System.Windows.Data;
using TPALCommander.Properties;

namespace TPALCommander
{
    public class CultureResources
    {
        private static ObjectDataProvider objectDataProvider =
            (ObjectDataProvider) App.Current.FindResource("Resources");

        public Resources GetResourceInstance()
        {
            return new Resources();
        }

        public static void ChangeCulture(CultureInfo culture)
        {
            Resources.Culture = culture;
            objectDataProvider.Refresh();
        }
    }
}
