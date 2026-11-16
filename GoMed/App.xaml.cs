using System.Windows;
using System.Windows.Media;
using GoMed.Data;
using Microsoft.EntityFrameworkCore;

namespace GoMed
{
    public partial class App : Application
    {
        public static bool EsModoOscuro { get; private set; } = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var context = new GoMed.Data.AppContext())
            {
                context.Database.Migrate();
            }
        }

        public static void AlternarTema()
        {
            AplicarTema(!EsModoOscuro);
        }

        public static void AplicarTema(bool oscuro)
        {
            EsModoOscuro = oscuro;

            if (oscuro)
            {
                // Paleta Modo Oscuro (Slate Dark)
                SetColor("AppBackgroundBrush", "#0F172A");
                SetColor("SurfaceBrush", "#1E293B");
                SetColor("SurfaceAltBrush", "#0F172A");
                SetColor("InputBackgroundBrush", "#0F172A");
                SetColor("TextDarkBrush", "#F8FAFC");
                SetColor("TextSecondaryBrush", "#94A3B8");
                SetColor("TextLightBrush", "#CBD5E1");
                SetColor("PlaceholderTextBrush", "#64748B");
                SetColor("BorderLightBrush", "#334155");
                SetColor("BorderCardBrush", "#334155");
                SetColor("GridLineBrush", "#1E293B");
                SetColor("RowHoverBrush", "#26354D");
                SetColor("RowSelectedBrush", "#1E3A5F");
                SetColor("CancelButtonBrush", "#1E293B");
                SetColor("CancelButtonBorderBrush", "#475569");
                SetColor("SidebarTextBrush", "#CBD5E1");
                SetColor("SidebarBackgroundHoverBrush", "#334155");
                SetColor("SidebarBackgroundActiveBrush", "#1E3A5F");
                SetColor("LightBlueHover", "#1E3A5F");
                SetColor("PrimaryBlueBrush", "#3B82F6");
                SetColor("WarningBgBrush", "#422006");
                SetColor("WarningTextBrush", "#FDE68A");
            }
            else
            {
                // Paleta Modo Claro Original GoMed
                SetColor("AppBackgroundBrush", "#F8F9FA");
                SetColor("SurfaceBrush", "#FFFFFF");
                SetColor("SurfaceAltBrush", "#FAFAFA");
                SetColor("InputBackgroundBrush", "#FFFFFF");
                SetColor("TextDarkBrush", "#2C3E50");
                SetColor("TextSecondaryBrush", "#7F8C8D");
                SetColor("TextLightBrush", "#666666");
                SetColor("PlaceholderTextBrush", "#94A3B8");
                SetColor("BorderLightBrush", "#CED4DA");
                SetColor("BorderCardBrush", "#E0E0E0");
                SetColor("GridLineBrush", "#F0F0F0");
                SetColor("RowHoverBrush", "#F8F9FA");
                SetColor("RowSelectedBrush", "#E8F4FF");
                SetColor("CancelButtonBrush", "#F8F9FA");
                SetColor("CancelButtonBorderBrush", "#6C757D");
                SetColor("SidebarTextBrush", "#495057");
                SetColor("SidebarBackgroundHoverBrush", "#F1F3F5");
                SetColor("SidebarBackgroundActiveBrush", "#E7F3FF");
                SetColor("LightBlueHover", "#F1F8FF");
                SetColor("PrimaryBlueBrush", "#007BFF");
                SetColor("WarningBgBrush", "#FFF3CD");
                SetColor("WarningTextBrush", "#856404");
            }
        }

        private static void SetColor(string key, string hex)
        {
            if (Current?.Resources != null)
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                Current.Resources[key] = new SolidColorBrush(color);
            }
        }
    }
}