using nio2so.TSOView2.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace nio2so.TSOView2
{
    /// <summary>
    /// Represents the current completion status for a task in TSOView2
    /// </summary>
    public struct TSOView2LoadingStatus
    {
        public TSOView2LoadingStatus(string overallTaskName, string currentTask, double overallProgress, bool Completed, double TaskProgress = .1) : this()
        {
            ArgumentException.ThrowIfNullOrEmpty(overallTaskName);
            ArgumentException.ThrowIfNullOrEmpty(currentTask);
            OverallTaskName = overallTaskName;
            CurrentTask = currentTask;
            OverallProgress = overallProgress;
            this.Completed = Completed;
            this.TaskProgress = TaskProgress;
        }

        public string OverallTaskName { get; set; }
        public string CurrentTask { get; }
        public double TaskProgress { get; }
        public double OverallProgress { get; }
        public bool Completed { get; }
    }
    /// <summary>
    /// An interface for exposing Window functions to a <see cref="ITSOView2Page"/>
    /// </summary>
    public interface ITSOView2Window
    {
        void ShowPlugin(Page NewPage);
        void ClosePlugin();
        Task ShowLoadingProgress(TSOView2LoadingStatus Status);
        void HideLoadingProgress();
    }
    /// <summary>
    /// A <see cref="Page"/> intended for use in a rich application window as the primary content.
    /// <para/>This has access to window functions through the <see cref="ParentWindow"/> property
    /// </summary>
    internal interface ITSOView2Page
    {
        /// <summary>
        /// Allows easy integration of Window-level features like loading bars, redirections, etc.
        /// </summary>
        public ITSOView2Window ParentWindow { get; set; }
    }

    public abstract class TSOView2WindowBase : Window, ITSOView2Window
    {
        private Window? _loadingWindow;
        private Dictionary<Type, ITSOViewPlugin?> _pluginInstances = new();

        public void HideLoadingProgress()
        {
            _loadingWindow?.Close();
            _loadingWindow = null;
            IsEnabled = true;
        }

        public async Task ShowLoadingProgress(TSOView2LoadingStatus Status)
        {
            await Dispatcher.InvokeAsync(delegate
            {
                //**create a loading window
                IsEnabled = false;

                if (_loadingWindow == null)
                {
                    DockPanel contentPanel = new DockPanel()
                    {
                        Margin = new Thickness(10)
                    };
                    TextBlock statusBlock = new TextBlock()
                    {
                        Text = "Getting started...",
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    DockPanel.SetDock(statusBlock, Dock.Top);
                    contentPanel.Children.Add(statusBlock);

                    ProgressBar totalProgress = new ProgressBar()
                    {
                        Height = 20,
                        Minimum = 0,
                        Maximum = 1
                    };
                    DockPanel.SetDock(totalProgress, Dock.Bottom);
                    contentPanel.Children.Add(totalProgress);

                    _loadingWindow = new Window()
                    {
                        Width = 350,
                        Height = 100,
                        Title = Status.OverallTaskName,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        WindowStyle = WindowStyle.ToolWindow,
                        Padding = new Thickness(10),
                        Content = contentPanel,
                        Owner = this
                    };
                }
                _loadingWindow.Show();
                //**

                Focus();
                if (Status.Completed)
                {
                    HideLoadingProgress();
                    return;
                }

                ((_loadingWindow.Content as DockPanel).Children[0] as TextBlock).Text = Status.CurrentTask;
                ((_loadingWindow.Content as DockPanel).Children[1] as ProgressBar).Value = Status.OverallProgress;
            });
        }
        public IEnumerable<MenuItem> UpdatePlugins()
        {
            _pluginInstances.Clear();             

            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> plugins = thisAssembly.GetTypes().Where(x => x.IsAssignableTo(typeof(ITSOViewPlugin)));
            foreach (var pluginType in plugins)
            {
                if (!pluginType.GetConstructors().Any()) continue;

                ITSOViewPlugin? instance = (ITSOViewPlugin)Activator.CreateInstance(pluginType);

                MenuItem item = new MenuItem()
                {
                    Header = instance?.PluginName ?? $"{pluginType.Name} (Unavailable)",
                    Tag = pluginType,
                    ToolTip = instance?.PluginDescription ?? "Cannot get plugin details.",
                };
                item.Click += PluginSelected;
                _pluginInstances.Add(pluginType, instance);
                yield return item;
            }
        }
        private void PluginSelected(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem) return;
            if (menuItem.Tag == null) return;
            if (!_pluginInstances.TryGetValue((Type)menuItem.Tag, out ITSOViewPlugin plugin)) return;
            if (plugin == null) return;
            plugin.Do(this);
        }

        public abstract void ShowPlugin(Page NewPage);
        public abstract void ClosePlugin();
    }
}
