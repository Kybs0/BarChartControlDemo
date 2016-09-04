using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BarChartControlDemo
{
    /// <summary>
    /// Interaction logic for BarChartControl.xaml
    /// </summary>
    public partial class BarChartControl : UserControl
    {
        public BarChartControl()
        {
            InitializeComponent();
        }

        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush",
        typeof(Brush), typeof(BarChartControl),
        new PropertyMetadata(Brushes.Black));

        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness",
        typeof(Thickness), typeof(BarChartControl),
        new PropertyMetadata(new Thickness(1.0, 0.0, 1.0, 1.0)));

        public AxisYModel AxisY
        {
            get { return (AxisYModel)GetValue(AxisYProperty); }
            set { SetValue(AxisYProperty, value); }
        }

        public static readonly DependencyProperty AxisYProperty = DependencyProperty.Register("AxisY",
        typeof(AxisYModel), typeof(BarChartControl),
        new PropertyMetadata(new AxisYModel()));

        public AxisXModel AxisX
        {
            get { return (AxisXModel)GetValue(AxisXProperty); }
            set { SetValue(AxisXProperty, value); }
        }

        public static readonly DependencyProperty AxisXProperty = DependencyProperty.Register("AxisX",
        typeof(AxisXModel), typeof(BarChartControl),
        new PropertyMetadata(new AxisXModel()));
        public double HeaderHeight
        {
            get { return (double)GetValue(HeaderHeightProperty); }
            set { SetValue(HeaderHeightProperty, value); }
        }
        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register("HeaderHeight",
        typeof(double), typeof(BarChartControl), new PropertyMetadata(10.0));
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header",
        typeof(string), typeof(BarChartControl), new PropertyMetadata());

        private void BarChartControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            MainBorder.BorderBrush = BorderBrush;
            MainBorder.BorderThickness = BorderThickness;

            HeaderGrid.Height = HeaderHeight;
            MainBarContent.Height = MyScrollBoxFrom0To1.ActualHeight - 17;
            LeftGrid.Width = AxisY.Width;

            SetYTitlesContent();

            SetXDatasContent();
        }

        private void SetXDatasContent()
        {
            var axisXModel = AxisX;
            if (axisXModel.Datas.Count > 0)
            {
                int count = axisXModel.Datas.Count;
                double barArea = axisXModel.BarWidth + axisXModel.MarginWidth * 2;

                for (int i = 0; i < count + 1; i++)
                {
                    MainBarContent.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(barArea)
                    });
                }
                int index = 0;
                foreach (var data in axisXModel.Datas)
                {
                    //主内容
                    var stackPanel = new StackPanel();
                    stackPanel.Orientation = Orientation.Vertical;

                    var tbl = new TextBlock();
                    tbl.Height = 15;
                    tbl.Text = data.Value.ToString();
                    tbl.Foreground = axisXModel.ForeGround;
                    tbl.HorizontalAlignment = HorizontalAlignment.Center;
                    stackPanel.Children.Add(tbl);

                    var radioButton = new RadioButton();
                    radioButton.Template = (ControlTemplate)this.Resources["LightedBtnTemplate"];
                    radioButton.Width = axisXModel.BarWidth;
                    double maxValue = AxisY.Titles.Max(i => i.Value);
                    radioButton.Height = (data.Value / maxValue) * (this.ActualHeight - BottomGrid.Height - HeaderHeight + 10);
                    var linearBrush = new LinearGradientBrush()
                    {
                        StartPoint = new Point(1, 0),
                        EndPoint = new Point(1, 1),
                        GradientStops = new GradientStopCollection() { 
                                            new GradientStop()
                                            {
                                                Color = data.FillBrush, Offset = 0
                                            }, new GradientStop()
                                            {
                                                Color = data.FillEndBrush, Offset = 1
                                            }
                                        }       
                    };
                    radioButton.Background = linearBrush;
                    radioButton.HorizontalAlignment = HorizontalAlignment.Center;
                    radioButton.Click += RadioButton_OnClick;
                    stackPanel.Children.Add(radioButton);

                    //底部
                    var bottomTbl = new TextBlock();
                    bottomTbl.Text = data.Name;
                    bottomTbl.Foreground = axisXModel.ForeGround;
                    bottomTbl.VerticalAlignment = VerticalAlignment.Center;
                    bottomTbl.TextAlignment = TextAlignment.Center;
                    bottomTbl.HorizontalAlignment = HorizontalAlignment.Center;
                    double textBlockWidth = axisXModel.LabelWidth;
                    bottomTbl.Width = axisXModel.LabelWidth;

                    stackPanel.Children.Add(bottomTbl);
                    Grid.SetColumn(stackPanel, index);
                    stackPanel.Margin = new Thickness(0, 0, -textBlockWidth / 2, -17);
                    stackPanel.VerticalAlignment = VerticalAlignment.Bottom;
                    stackPanel.HorizontalAlignment = HorizontalAlignment.Right;

                    MainBarContent.Children.Add(stackPanel);
                    MainBarContent.Background = Brushes.Transparent;
                    index++;
                }
                double mainBarContentWidth = count * barArea;
                if (mainBarContentWidth > MainGridFrom0To1Content.ActualWidth)
                {
                    BtnLeft.Visibility = Visibility.Visible;
                    BtnRight.Visibility = Visibility.Visible;
                }
            }

        }

        public delegate void BarSelectionChangedEventArgs(object sender, RoutedEventArgs e);

        public event BarSelectionChangedEventArgs BarSelectionChanged;
        private void RadioButton_OnClick(object sender, RoutedEventArgs e)
        {
            var currentButton = sender as RadioButton;
            foreach (var element in MainBarContent.Children)
            {
                var stackPanel = element as StackPanel;
                var button = stackPanel.Children[1] as RadioButton;
                if (button != currentButton)
                {
                    button.IsChecked = false;
                }
            }
            if (BarSelectionChanged!=null)
            {
                BarSelectionChanged(sender, e);
            }
        }
        private void SetYTitlesContent()
        {
            var axisYModel = AxisY;
            if (axisYModel.Titles.Count > 0)
            {
                int gridRows = axisYModel.Titles.Count - 1;
                for (int i = 0; i < gridRows; i++)
                {
                    LeftGrid.RowDefinitions.Add(new RowDefinition());
                    MainGridForRow1.RowDefinitions.Add(new RowDefinition());
                }
                int index = 0;
                foreach (var title in axisYModel.Titles)
                {
                    var textblock = new TextBlock();
                    textblock.Text = title.Name;
                    textblock.Foreground = axisYModel.ForeGround;
                    textblock.HorizontalAlignment = HorizontalAlignment.Right;
                    textblock.Height = axisYModel.LabelHeight;
                    if (index < gridRows)
                    {
                        textblock.VerticalAlignment = VerticalAlignment.Bottom;
                        textblock.Margin = new Thickness(0, 0, 5, -axisYModel.LabelHeight / 2);//因为设置在行底部还不够,必须往下移
                        Grid.SetRow(textblock, gridRows - index - 1);
                    }
                    else
                    {
                        textblock.VerticalAlignment = VerticalAlignment.Top;
                        textblock.Margin = new Thickness(0, -axisYModel.LabelHeight / 2, 5, 0);//最后一个,设置在顶部
                        Grid.SetRow(textblock, 0);
                    }
                    LeftGrid.Children.Add(textblock);

                    var border = new Border();
                    border.Height = axisYModel.LineHeight;
                    border.BorderBrush = axisYModel.LineBrush;
                    double thickness = Convert.ToDouble(axisYModel.LineHeight) / 2;
                    border.BorderThickness = new Thickness(0, thickness, 0, thickness);
                    if (index < gridRows)
                    {
                        border.VerticalAlignment = VerticalAlignment.Bottom;
                        border.Margin = new Thickness(0, 0, 0, -thickness);//因为设置在行底部还不够,必须往下移
                        Grid.SetRow(border, gridRows - index - 1);
                    }
                    else
                    {
                        border.VerticalAlignment = VerticalAlignment.Top;
                        border.Margin = new Thickness(0, -thickness, 0, 0);//最后一个,设置在顶部
                        Grid.SetRow(border, 0);
                    }
                    Grid.SetColumn(border, 0);
                    Grid.SetColumnSpan(border, AxisX.Datas.Count + 1);
                    MainGridForRow1.Children.Add(border);
                    index++;
                }
            }
        }
        /// <summary>
        /// 设置分行
        /// </summary>
        /// <param name="leftGrid"></param>
        /// <param name="count"></param>
        private void SetGridRowDefinitions(Grid leftGrid, int count)
        {
            for (int i = 0; i < count; i++)
            {
                leftGrid.RowDefinitions.Add(new RowDefinition());
            }
        }
        /// <summary>
        /// 向左滑动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLeft_OnClick(object sender, RoutedEventArgs e)
        {
            var startOffset = MyScrollBoxFrom0To1.HorizontalOffset;
            if (startOffset <= _lastHorizontalOffset)
            {
                MyScrollBoxFrom0To1.HorizontalOffset = startOffset - 100;
                _lastHorizontalOffset = startOffset - 100;
            }
            else if (startOffset > _lastHorizontalOffset)
            {
                BtnLeft.Opacity = 0.5;
            }
            BtnRight.Opacity = 1;
        }

        private double _lastHorizontalOffset = 0;
        /// <summary>
        /// 向右滑动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRight_OnClick(object sender, RoutedEventArgs e)
        {
            var startOffset = MyScrollBoxFrom0To1.HorizontalOffset;
            if (startOffset >= _lastHorizontalOffset)
            {
                MyScrollBoxFrom0To1.HorizontalOffset = startOffset + 100;
                _lastHorizontalOffset = startOffset + 100;
            }
            else if (startOffset < _lastHorizontalOffset)
            {
                BtnRight.Opacity = 0.5;
            }
            BtnLeft.Opacity = 1;
        }
    }
    #region XDataModel
    public class AxisXModel
    {
        private double _labelWidth = 20;
        /// <summary>
        /// 底部标签-单个宽度
        /// </summary>
        public double LabelWidth
        {
            get { return _labelWidth; }
            set { _labelWidth = value; }
        }
        private double _marginWidth = 20;
        /// <summary>
        /// Bar间隔宽度
        /// </summary>
        public double MarginWidth
        {
            get { return _marginWidth; }
            set { _marginWidth = value; }
        }
        private double _height = 20;
        /// <summary>
        /// 高度
        /// </summary>
        public double Height
        {
            get
            {
                return _height;
            }
            set { _height = value; }
        }

        private Brush _foreGround = Brushes.Black;
        /// <summary>
        /// 字体颜色
        /// </summary>
        public Brush ForeGround
        {
            get { return _foreGround; }
            set { _foreGround = value; }
        }
        private double _barWidth = 30;
        /// <summary>
        /// Bar宽度
        /// </summary>
        public double BarWidth
        {
            get { return _barWidth; }
            set { _barWidth = value; }
        }
        List<AxisXDataModel> _datas = new List<AxisXDataModel>();
        /// <summary>
        /// 数据
        /// </summary>
        public List<AxisXDataModel> Datas
        {
            get { return _datas; }
            set { _datas = value; }
        }
    }
    public class AxisXDataModel : DataModel
    {

        private Color _fillBrush = Colors.Blue;
        /// <summary>
        /// Bar填充颜色
        /// </summary>
        public Color FillBrush
        {
            get
            {
                return _fillBrush;
            }
            set { _fillBrush = value; }
        }

        private Color _fillEndBrush = Colors.Blue;

        public Color FillEndBrush
        {
            get
            {
                return _fillEndBrush;
            }
            set { _fillEndBrush = value; }
        }
    }
    #endregion

    #region YDataModel
    public class AxisYModel
    {
        private double _width = 20;
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get { return _width; } set { _width = value; } }

        private Brush _foreGround = Brushes.Black;
        /// <summary>
        /// 字体颜色
        /// </summary>
        public Brush ForeGround { get { return _foreGround; } set { _foreGround = value; } }

        private double _labelHeight = 15;
        /// <summary>
        /// 左侧标题栏-单个标题高度
        /// </summary>
        public double LabelHeight
        {
            get { return _labelHeight; }
            set { _labelHeight = value; }
        }
        private double _lineHeight = 0.2;
        /// <summary>
        /// GridLine高度
        /// </summary>
        public double LineHeight
        {
            get { return _lineHeight; }
            set { _lineHeight = value; }
        }


        private Brush _lineBrush = Brushes.Blue;
        /// <summary>
        /// Bar填充颜色
        /// </summary>
        public Brush LineBrush
        {
            get { return _lineBrush; }
            set { _lineBrush = value; }
        }

        List<AxisYDataModel> _titles = new List<AxisYDataModel>();
        /// <summary>
        /// 左侧标题列表
        /// </summary>
        public List<AxisYDataModel> Titles
        {
            get { return _titles; }
            set { _titles = value; }
        }
    }
    public class AxisYDataModel : DataModel
    {

    }
    #endregion

    public class DataModel
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public double Value { get; set; }
    }
}