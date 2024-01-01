using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MTFApp.UIHelpers
{
    public abstract class MTFUserControl : UserControl
    {
        private void LoadCfg(Grid grid)
        {
            if (grid!=null)
            {
                ControlInfo controlConfig = StoreSettings.GetInstance.SettingsClass.ControlInfos.FirstOrDefault(item => item.Name == GetType().FullName);
                if (controlConfig!=null)
                {
                    LoadGrid(grid, controlConfig);
                }
            }
        }

        private void StoreCfg()
        {
            var settings = StoreSettings.GetInstance;

            ControlInfo controlConfig = settings.SettingsClass.ControlInfos.FirstOrDefault(item => item.Name == GetType().FullName);
            if (controlConfig == null)
            {
                controlConfig = new ControlInfo() { Name = GetType().FullName };
                settings.SettingsClass.ControlInfos.Add(controlConfig);
            }

            controlConfig.Values = new List<PropertyInfo>();

            UIHelper.FindVisualChildren<Grid>(this).ToList().ForEach(grid => StoreGrid(grid, controlConfig));
        }

        private static void LoadGrid(Grid grid, ControlInfo controlConfig)
        {
            if (!string.IsNullOrEmpty(grid.Name))
            {
                var values = controlConfig.Values.Where(item => item.Name.StartsWith(grid.Name + "columnDef")).ToList();
                for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                {
                    var value = values.FirstOrDefault(v => v.Name.EndsWith("[" + i + "]"));
                    if (value != null)
                    {
                        GridLength gl = new GridLength(double.Parse(value.Value));
                        grid.ColumnDefinitions[i].Width = gl;
                    }
                }

                values = controlConfig.Values.Where(item => item.Name.StartsWith(grid.Name + "rowDef")).ToList();
                for (int i = 0; i < grid.RowDefinitions.Count; i++)
                {
                    var value = values.FirstOrDefault(v => v.Name.EndsWith("[" + i + "]"));
                    if (value != null)
                    {
                        GridLength gl = new GridLength(double.Parse(value.Value));
                        grid.RowDefinitions[i].Height = gl;
                    }
                }
            }
        }

        private static void StoreGrid(Grid grid, ControlInfo controlConfig)
        {
            if (!string.IsNullOrEmpty(grid.Name))
            {
                for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                {
                    if (grid.ColumnDefinitions[i].Width.IsAbsolute)
                    {
                        controlConfig.Values.Add(new PropertyInfo()
                        {
                            Name = grid.Name + "columnDef[" + i + "]",
                            Value = grid.ColumnDefinitions[i].ActualWidth.ToString()
                        });
                    }

                }
                for (int i = 0; i < grid.RowDefinitions.Count; i++)
                {
                    if (grid.RowDefinitions[i].Height.IsAbsolute)
                    {
                        controlConfig.Values.Add(new PropertyInfo()
                        {
                            Name = grid.Name + "rowDef[" + i + "]",
                            Value = grid.RowDefinitions[i].ActualHeight.ToString()
                        });
                    }
                }
            }
        }

        protected virtual void GridSplitterDragCompleted(object sender, DragCompletedEventArgs e)
        {
            StoreCfg();
        }

        protected void GridOnLoaded(object sender, RoutedEventArgs e)
        {
            LoadCfg(sender as Grid);
        }
        
    }
}
