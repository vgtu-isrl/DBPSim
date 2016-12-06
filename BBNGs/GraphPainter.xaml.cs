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
using System.Windows.Shapes;
using BBNGs.Graph;

namespace BBNGs
{
    /// <summary>
    /// Interaction logic for GraphPainter.xaml
    /// </summary>
    public partial class GraphPainter : Window
    {
        CanvasPainter painter;
        internal GraphPainter(Tree tree)
        {
            InitializeComponent();

            painter = new CanvasPainter(PaintCanvas, this);
                foreach (Node n in tree.distinctNodes.Values)
                {
                    painter.AddNode(n);
                }
        }

        class CanvasPainter
        {
            Dictionary<Node, PaintNode> paintedNodes = new Dictionary<Node, PaintNode>();
            List<Node> beingPainted = new List<Node>();
            private Canvas cnv;
            private Window w;

            public CanvasPainter(Canvas c, Window w)
            {
                this.cnv = c;
                this.w = w;
            }
            public void AddNode(Node n)
            {
                PaintNode nn = null;
                if (paintedNodes.TryGetValue(n, out nn))
                {
                    return;
                }
                if (n.parentNodes.Count == 0)
                {
                    nn = new PaintNode(paintedNodes, cnv, n);
                    var paintedRoots = paintedNodes.Keys.Where(x => x.parentNodes.Count == 0);
                    double maxLeft = paintedRoots.FirstOrDefault() != null ? paintedRoots.Max(x => paintedNodes[x].rightPos) + 10 : 10;
                    nn.SetLocation(maxLeft, 5);
                    nn.Show();
                    paintedNodes.Add(n, nn);
                }
                else
                {


                    nn = new PaintNode(paintedNodes, cnv, n);
                    if (paintedNodes.ContainsKey(n))
                    {
                        paintedNodes[n].Element.Fill = Brushes.Red;
                        return;
                    }
                    paintedNodes.Add(n, nn);
                    foreach (Node pn in n.parentNodes)
                    {
                        beingPainted.Add(pn);
                        AddNode(pn);
                        beingPainted.Remove(pn);
                    }
                    
                    double maxTop = n.parentNodes.Where(x=>paintedNodes.ContainsKey(x)).Max(x => paintedNodes[x].bottomPos) + 10;
                    var paintedSameLevel = paintedNodes.Where(x => x.Value.top == maxTop).Select(x => x.Key);
                    double maxLeft = paintedSameLevel.FirstOrDefault() != null ? paintedSameLevel.Max(x => paintedNodes[x].rightPos) + 10 : 10;
                    n.parentNodes.Where(x => paintedNodes.ContainsKey(x)).ToList().ForEach(x =>
                    {
                        nn.AssociateParentNode(paintedNodes[x]);
                    });
                    nn.SetLocation(maxLeft, maxTop);
                    nn.Show();
                }
                w.Width = paintedNodes.Max(x => x.Value.rightPos) + 100;
                w.Height = paintedNodes.Max(x => !double.IsNaN(x.Value.top) ? x.Value.top : 20) + 100;
                cnv.Width = w.Width;
                cnv.Height = w.Height;
            }

            public void ShowAll()
            {
                foreach (var pn in paintedNodes)
                {
                    pn.Value.Show();
                }
            }
        }


        class PaintNode
        {
            private Ellipse element;

            public Ellipse Element { get { return element; } }
            private Label label;
            private Dictionary<PaintNode, Arc> lines = new Dictionary<PaintNode, Arc>();
            private Canvas cnv;
            private Node n;
            private Dictionary<Node, PaintNode> paintedNodes = new Dictionary<Node, PaintNode>();

            public double top { get; private set; }
            public double left { get; private set; }
            public double rightPos { get { return left + element.Width; } }
            public double bottomPos { get { return top + element.Height; } }
            public bool visible;


            public PaintNode(Dictionary<Node, PaintNode> paintedNodes, Canvas canvas, Node n)
            {
                element = new Ellipse();
                element.Width = n.name.Length * 6.2;
                element.Height = 50;
                element.Stroke = Brushes.Black;
                element.StrokeThickness = 1;
                if(element.Fill != Brushes.Red)
                {
                    element.Fill = Brushes.LightGray;
                }
                label = new Label();

                label.Content = n.name;
                this.n = n;
                cnv = canvas;
                top = 0;
                left = 0;
                this.paintedNodes = paintedNodes;
                element.MouseLeftButtonUp += element_MouseLeftButtonUp;
            }

            void element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                var nodes = this.n.childNodes.Union(this.n.parentNodes);

                foreach (var pn in paintedNodes)
                {
                    if (pn.Key == this.n)
                    {
                        pn.Value.Show();
                    }
                    else if (!nodes.Contains(pn.Key))
                    {
                        pn.Value.Hide();
                    }
                    else
                    {
                        pn.Value.Show();
                    }
                }
                e.Handled = true;
            }

            public void AssociateParentNode(PaintNode parent)
            {
                Arc r = new Arc(cnv, this, parent);
                lines.Add(parent, r);
            }

            public void SetLocation(double left, double top)
            {
                this.left = left;
                this.top = top;
                Canvas.SetTop(this.element, top);
                Canvas.SetLeft(this.element, left);
                Canvas.SetTop(this.label, top + 10);
                Canvas.SetLeft(this.label, left + 5);

                foreach (var arc in lines.Values)
                {
                    arc.Show();
                }
            }

            public void Show()
            {
                this.element.Visibility = Visibility.Visible;
                this.label.Visibility = Visibility.Visible;
                visible = true;
                if (!cnv.Children.Contains(this.element))
                {
                    cnv.Children.Add(this.element);
                    cnv.Children.Add(this.label);
                }
                foreach (var arc in lines.Values)
                {
                    arc.Show();
                }
            }

            public void Hide()
            {
                this.element.Visibility = Visibility.Hidden;
                this.label.Visibility = Visibility.Hidden;
                visible = false;
                foreach (var arc in lines.Values)
                {
                    arc.Hide();
                }
            }
        }

        class Arc
        {
            public Line ln;
            public Ellipse arrowEnd;
            public PaintNode parentNode;
            public PaintNode childNode;
            private Canvas cnv;

            public Arc(Canvas cnv, PaintNode parent, PaintNode child)
            {
                this.parentNode = parent;
                this.childNode = child;
                this.cnv = cnv;
                parent.Element.IsVisibleChanged += Element_IsVisibleChanged;
                child.Element.IsVisibleChanged += Element_IsVisibleChanged;

                ln = new Line();
                ln.Stroke = Brushes.Black;
                ln.StrokeThickness = 1;
                arrowEnd = new Ellipse();
                arrowEnd.Stroke = Brushes.Black;
                arrowEnd.Fill = Brushes.Black;
                arrowEnd.StrokeThickness = 1;
                arrowEnd.Width = 5;
                arrowEnd.Height = 5;

            }

            void Element_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                Ellipse parent = (Ellipse)sender;
                if (parent.IsVisible)
                {
                    this.Show();
                }
                else
                {
                    this.Hide();
                }
            }

            public void Show()
            {
                if (!parentNode.visible || !childNode.visible)
                {
                    return;
                }
                this.ln.StrokeThickness = 1;
                this.arrowEnd.StrokeThickness = 1;
                if (!cnv.Children.Contains(this.ln))
                {
                    cnv.Children.Add(this.ln);
                    cnv.Children.Add(this.arrowEnd);
                }
                ResetLocation();
            }

            public void Hide()
            {
                this.ln.StrokeThickness = 0;
                this.arrowEnd.StrokeThickness = 0;
                this.arrowEnd.Fill = Brushes.White;
            }

            public void ResetLocation()
            {

                ln.X1 = (parentNode.left + parentNode.rightPos) / 2;
                ln.X2 = (childNode.left + childNode.rightPos) / 2;
                ln.Y1 = parentNode.top;
                ln.Y2 = childNode.bottomPos;

                //Canvas.SetTop(ln,parentNode.top);
                //Canvas.SetLeft(ln,parentNode.left);
                Canvas.SetLeft(arrowEnd, ln.X1);
                Canvas.SetTop(arrowEnd, ln.Y1);
            }
        }

        DateTime prevClick = DateTime.Now;
        private void PaintCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - prevClick) > new TimeSpan(0, 0, 0, 0, 100))
            {
                painter.ShowAll();
                prevClick = DateTime.Now;
            }
            else
            {
                prevClick = DateTime.Now;
            }
        }




    }
}
