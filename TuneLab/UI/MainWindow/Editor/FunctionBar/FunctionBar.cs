﻿using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuneLab.Audio;
using TuneLab.Base.Event;
using TuneLab.Base.Properties;
using TuneLab.Data;
using TuneLab.GUI;
using TuneLab.GUI.Components;
using TuneLab.GUI.Controllers;
using TuneLab.I18N;
using TuneLab.Utils;
using static TuneLab.Base.Science.MusicTheory;

namespace TuneLab.UI;

internal class FunctionBar : LayerPanel
{
    public event Action<double>? Moved;
    public INotifiableProperty<bool> IsAutoPage { get; } = new NotifiableProperty<bool>(false);

    public IActionEvent<QuantizationBase, QuantizationDivision> QuantizationChanged => mQuantizationChanged;

    public interface IDependency
    {
        public INotifiableProperty<PianoTool> PianoTool { get; }
    }

    public FunctionBar(IDependency dependency)
    {
        mDependency = dependency;

        var mover = new Mover() { Margin = new(0, 1) };
        mover.Moved.Subscribe(p => Moved?.Invoke(p.Y + Bounds.Y));
        Children.Add(mover);

        var dockPanel = new DockPanel() { Margin = new(64, 0, 360, 0) };
        {
            var hoverBack = Colors.White.Opacity(0.05);

            void SetupToolTip(Toggle toggleButton,string ToolTipText)
            {
                ToolTip.SetPlacement(toggleButton, PlacementMode.Top);
                ToolTip.SetVerticalOffset(toggleButton, -8);
                ToolTip.SetShowDelay(toggleButton, 0);
                ToolTip.SetTip(toggleButton, ToolTipText);
            }

            var audioControlPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 12, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new(12, 0) };
            {
                var playButtonIconItem = new IconItem() { Icon = Assets.Play };
                var playButton = new Toggle() { Width = 36, Height = 36 }
                    .AddContent(new() { Item = new BorderItem() { CornerRadius = 4 }, CheckedColorSet = new() { HoveredColor = hoverBack, PressedColor = hoverBack }, UncheckedColorSet = new() { HoveredColor = hoverBack, PressedColor = hoverBack } })
                    .AddContent(new() { Item = playButtonIconItem, CheckedColorSet = new() { Color = Colors.White }, UncheckedColorSet = new() { Color = Style.LIGHT_WHITE.Opacity(0.5) } });
                
                SetupToolTip(playButton, "Play".Tr(this));
                playButton.Switched.Subscribe(() => { if (playButton.IsChecked) AudioEngine.Play(); else AudioEngine.Pause(); SetupToolTip(playButton, AudioEngine.IsPlaying ? "Pause".Tr(this) : "Play".Tr(this)); });
                AudioEngine.PlayStateChanged += () => { playButtonIconItem.Icon = AudioEngine.IsPlaying ? Assets.Pause : Assets.Play; playButton.Display(AudioEngine.IsPlaying); SetupToolTip(playButton, AudioEngine.IsPlaying ? "Pause".Tr(this) : "Play".Tr(this));};
                audioControlPanel.Children.Add(playButton);

                var autoPageButton = new Toggle() { Width = 36, Height = 36 }
                    .AddContent(new() { Item = new BorderItem() { CornerRadius = 4 }, CheckedColorSet = new() { Color = Style.HIGH_LIGHT }, UncheckedColorSet = new() { HoveredColor = hoverBack, PressedColor = hoverBack } })
                    .AddContent(new() { Item = new IconItem() { Icon = Assets.AutoPage }, CheckedColorSet = new() { Color = Colors.White }, UncheckedColorSet = new() { Color = Style.LIGHT_WHITE.Opacity(0.5) } });

                SetupToolTip(autoPageButton, "Auto Scroll".Tr(this));
                autoPageButton.Bind(IsAutoPage);
                audioControlPanel.Children.Add(autoPageButton);
            }
            dockPanel.AddDock(audioControlPanel, Dock.Left);

            var quantizationPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 12, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            {
                var quantizationLabel = new TextBlock() { Text = "Quantization".Tr(this) + ": ", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                quantizationPanel.Children.Add(quantizationLabel);
                var quantizationComboBox = new ComboBoxController() { Width = 96 };
                (string option, QuantizationBase quantizationBase, QuantizationDivision quantizationDivision)[] options = 
                [
                    ("1/1", QuantizationBase.Base_1, QuantizationDivision.Division_1),
                    ("1/2", QuantizationBase.Base_1, QuantizationDivision.Division_2),
                    ("1/4", QuantizationBase.Base_1, QuantizationDivision.Division_4),
                    ("1/8", QuantizationBase.Base_1, QuantizationDivision.Division_8),
                    ("1/16", QuantizationBase.Base_1, QuantizationDivision.Division_16),
                    ("1/32", QuantizationBase.Base_1, QuantizationDivision.Division_32),
                    ("1/3", QuantizationBase.Base_3, QuantizationDivision.Division_1),
                    ("1/6", QuantizationBase.Base_3, QuantizationDivision.Division_2),
                    ("1/12", QuantizationBase.Base_3, QuantizationDivision.Division_4),
                    ("1/24", QuantizationBase.Base_3, QuantizationDivision.Division_8),
                    ("1/48", QuantizationBase.Base_3, QuantizationDivision.Division_16),
                    ("1/96", QuantizationBase.Base_3, QuantizationDivision.Division_32),
                    ("1/5", QuantizationBase.Base_5, QuantizationDivision.Division_1),
                    ("1/10", QuantizationBase.Base_5, QuantizationDivision.Division_2),
                    ("1/20", QuantizationBase.Base_5, QuantizationDivision.Division_4),
                    ("1/40", QuantizationBase.Base_5, QuantizationDivision.Division_8),
                    ("1/80", QuantizationBase.Base_5, QuantizationDivision.Division_16),
                    ("1/160", QuantizationBase.Base_5, QuantizationDivision.Division_32),
                ];
                quantizationComboBox.SetConfig(new(options.Select(option => option.option).ToList(), 3));
                quantizationComboBox.ValueCommited.Subscribe(() => { var index = quantizationComboBox.Index; if ((uint)index >= options.Length) return; mQuantizationChanged.Invoke(options[index].quantizationBase, options[index].quantizationDivision); });
                quantizationPanel.Children.Add(quantizationComboBox);
            }
            dockPanel.AddDock(quantizationPanel, Dock.Right);

            var pianoToolPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 12, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            {
                void AddButton(PianoTool tool, SvgIcon icon, string tipText)
                {
                    var toggle = new Toggle() { Width = 36, Height = 36 }
                        .AddContent(new() { Item = new BorderItem() { CornerRadius = 4 }, CheckedColorSet = new() { Color = Style.HIGH_LIGHT }, UncheckedColorSet = new() { HoveredColor = hoverBack, PressedColor = hoverBack } })
                        .AddContent(new() { Item = new IconItem() { Icon = icon }, CheckedColorSet = new() { Color = Colors.White }, UncheckedColorSet = new() { Color = Style.LIGHT_WHITE.Opacity(0.5) } });
                    void OnPianoToolChanged()
                    {
                        toggle.Display(mDependency.PianoTool.Value == tool);
                    }
                    SetupToolTip(toggle, tipText);
                    toggle.AllowSwitch += () => !toggle.IsChecked;
                    toggle.Switched.Subscribe(() => mDependency.PianoTool.Value = tool);
                    mDependency.PianoTool.Modified.Subscribe(OnPianoToolChanged);
                    pianoToolPanel.Children.Add(toggle);
                    OnPianoToolChanged();
                }
                AddButton(PianoTool.Note, Assets.Pointer, "Note Tool".Tr(this));
                AddButton(PianoTool.Pitch, Assets.Pitch, "Pitch Pen".Tr(this));
                AddButton(PianoTool.Anchor, Assets.Anchor, "Anchor Tool".Tr(this));
                AddButton(PianoTool.Lock, Assets.Brush, "Pitch Locking Brush".Tr(this));
                AddButton(PianoTool.Vibrato, Assets.Vibrato, "Vibrato Tool".Tr(this));
                AddButton(PianoTool.Select, Assets.Select, "Selection Tool".Tr(this));
            }
            dockPanel.AddDock(pianoToolPanel);
        }
        Children.Add(dockPanel);

        Height = 60;
        Background = Style.BACK.ToBrush();
    }

    class Mover : MovableComponent
    {
        public override void Render(DrawingContext context)
        {
            context.FillRectangle(Style.INTERFACE.ToBrush(), this.Rect());
        }
    }

    readonly ActionEvent<QuantizationBase, QuantizationDivision> mQuantizationChanged = new();

    readonly IDependency mDependency;
}
