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
using System.IO;
using System.Data;
using System.Collections;


using BBNGs.Graph;
using BBNGs.Engine;
using BBNGs.TraceLog;
using BBNGs.Util;

namespace BBNGs
{
    /// <summary>
    /// Interaction logic for PredictionProgress.xaml
    /// </summary>
    public partial class PredictionProgress : Window
    {

        System.Threading.Timer tmr;

        internal PredictionProgress(BBNGine eng)
        {
            InitializeComponent();
            engine = eng;
            int idx = 0;
            engine.traceTree.distinctNodes.Values.OrderBy(x => x.name).ToList().ForEach(
                x =>
                {
                    success.Add(x, new KeyValuePair<int, int>(0, 0));
                    wrong.Add(x, new KeyValuePair<int, int>(0, 0));
                    partials.Add(x, new KeyValuePair<int, int>(0, 0));
                    progressbars.Add(new ProgressView(x, success, wrong, partials, idx, PaintCanvas));
                    idx++;
                });
            this.Width = 600;
            PaintCanvas.Width = 600;
            PaintCanvas.Height = idx * 15 + 50;
            PredictionStart();
            tmr = new System.Threading.Timer(new System.Threading.TimerCallback(Repaint));
            tmr.Change(500, 50);
        }


        BBNGine engine;



        Dictionary<Node, KeyValuePair<int, int>> success = new Dictionary<Node, KeyValuePair<int, int>>();
        Dictionary<Node, KeyValuePair<int, int>> wrong = new Dictionary<Node, KeyValuePair<int, int>>();
        Dictionary<Node, KeyValuePair<int, int>> partials = new Dictionary<Node, KeyValuePair<int, int>>();
        List<ProgressView> progressbars = new List<ProgressView>();
        int total = 0;
        int completed = 0;
        int totalCorrect = 0;
        int totalEvents = 0;

        private void PredictionStart()
        {
            new Task(
              new Action(delegate ()
              {
                  Dictionary<Node, bool> workers = new Dictionary<Node, bool>();

                  //engine.probs.GenerateDependencies();

                  List<Trace> trcs = engine.TestSet;
                  total = trcs.Count;

                  using (StreamWriter sw = new StreamWriter(File.Open(Environment.CurrentDirectory + @"\replay.txt", FileMode.Create, FileAccess.ReadWrite)))
                  {
                      int i = 0;

                      var correctPredictions = new List<PredictionResult>();
                      foreach (Trace t in trcs)
                      {
                          CalculatePrediction(i, correctPredictions, t);
                          i++;
                          completed = i;
                      }


                      using (StreamWriter sw2 = new StreamWriter(File.Open(Environment.CurrentDirectory + @"\replay_content.txt", FileMode.Create, FileAccess.ReadWrite)))
                      {
                          var cps = correctPredictions.GroupBy(x => x.currentEvent.name);
                          foreach (var cp in cps)
                          {
                              var nxt = cp.GroupBy(x => x.nextEvent.name);
                              foreach (var n in nxt)
                              {
                                  sw.WriteLine("//Sequence " + cp.Key + "=>" + n.Key);
                                  HashSet<string> uniqueVals = new HashSet<string>();
                                  List<List<string>> sets = new List<List<string>>();

                                  foreach(var y in n)
                                  {
                                      List<string> set = new List<string>();
                                      foreach(var v in y.currentEvent.EventVals)
                                      {
                                          var vv = v.Key + "=" + v.Value.value;
                                          if (!uniqueVals.Contains(vv))
                                          {
                                              uniqueVals.Add(vv);
                                          }
                                          set.Add(vv);
                                      }
                                      sets.Add(set);
                                  }
                                  Utilities.Apriori.IApriori apr = new Utilities.Apriori.Apriori();
                                  var output = apr.ProcessTransaction(0.2, 0.6, uniqueVals.Select(x=>new List<string>() {x }).ToList(), sets);

                                 

                                  sw.WriteLine("->Rules for sequence");
                                  foreach(var rule in output.StrongRules)
                                  {
                                      sw.WriteLine(rule.Confidence + ":" + String.Join(";", rule.X) + ";" + String.Join(";", rule.Y));
                                  }

                                  var p = 0;
                              }
                          }
                      }
                      Utilities.TransformationUtility.TransformRulesIntoHtml(Environment.CurrentDirectory + @"\replay_content.txt", Environment.CurrentDirectory + @"\replay_content.html");

                      foreach (var s in success)
                      {
                          var correct = ((double)s.Value.Key) / ((double)s.Value.Value);
                          var partial = (((double)partials[s.Key].Key) / ((double)partials[s.Key].Value));
                          var wr = (((double)wrong[s.Key].Key) / ((double)wrong[s.Key].Value));
                          var missed = 1 - correct - partial - wr;
                          sw.WriteLine(s.Key.name + "\t" + correct + "\t" + partial + "\t" + s.Value.Value);
                      }                      
                  }
              })
              ).Start();
        }

        private void CalculatePrediction(int i, List<PredictionResult> correctPredictions, Trace t)
        {
            List<Node> partialCorrects = new List<Node>();
            var res = engine.probs.TracePredictionStepByStep(t);


            int correct = 0;
            int tot = 0;
            int partialCorrect = 0;

            foreach (var s in res.results)
            {
                #region generate result string
                StringBuilder sb = new StringBuilder();
                sb.Append(">" + s.Value.correct.ToString().Remove(1) + "#" + s.Value.failedPredictionWithOccurence.ToString().Remove(1) + "#");
                sb.Append("P(" + s.Value.current + ")=" + s.Value.currentProb);
                sb.Append("#P(" + (s.Value.bestNextChoice == null ? "NaN" : s.Value.bestNextChoice.name)
                    + ")="
                    + s.Value.nextProb
                    + "#" + (s.Value.next == null ? "NaN" : ("P(" + s.Value.next.name + ")=" + s.Value.realNextProb)));
                // sw.WriteLine(sb.ToString());
                #endregion
                tot++;

                if (s.Value.correct)
                {
                    correctPredictions.Add(s.Value);
                }

                RecalculatePredictionResults(partialCorrects, ref correct, ref partialCorrect, s);

            }
            this.totalCorrect += correct;
            this.totalEvents += tot;
            string resString = "Predicted:" + i.ToString() + " width " + partialCorrect + "#" + correct.ToString() + "#" + (res.results.Count - 1).ToString();
        }

        private void RecalculatePredictionResults(List<Node> partialCorrects, ref int correct, ref int partialCorrect, KeyValuePair<Node, PredictionResult> s)
        {
            var guessedActual = success[s.Value.next];
            var guessCurrent = s.Value.bestNextChoice != null ? success[s.Value.bestNextChoice] : default(KeyValuePair<int, int>);
            var partialCurrent = s.Value.bestNextChoice != null ? partials[s.Value.bestNextChoice] : default(KeyValuePair<int, int>);
            var wrongCurrent = s.Value.bestNextChoice != null ? wrong[s.Value.bestNextChoice] : default(KeyValuePair<int, int>);

            bool partial = false;
            if (s.Value.failedPredictionWithOccurence)
            {
                if (!partialCorrects.Contains(s.Value.bestNextChoice))
                {
                    partials[s.Value.bestNextChoice] = new KeyValuePair<int, int>(partialCurrent.Key + 1, partialCurrent.Value + 1);
                    partialCorrect++;
                    partialCorrects.Add(s.Value.bestNextChoice);
                }
                partial = true;
            }
            else if (s.Value.bestNextChoice != null)
            {
                partials[s.Value.bestNextChoice] = new KeyValuePair<int, int>(partialCurrent.Key, partialCurrent.Value + 1);
            }

            if (s.Value.correct)
            {
                AddCorrectPredictionResult(partialCorrects, ref correct, ref partialCorrect, s, guessCurrent, partialCurrent, wrongCurrent);
            }
            else
            {
                AddWrongPredictionResult(s, guessedActual, wrongCurrent, partial);
            }
        }

        private void AddWrongPredictionResult(KeyValuePair<Node, PredictionResult> s, KeyValuePair<int, int> guessedActual, KeyValuePair<int, int> wrongCurrent, bool partial)
        {
            success[s.Value.next] = new KeyValuePair<int, int>(guessedActual.Key, guessedActual.Value + 1);
            if (s.Value.bestNextChoice != null && !partial)
            {
                wrong[s.Value.bestNextChoice] = new KeyValuePair<int, int>(wrongCurrent.Key + 1, wrongCurrent.Value + 1);
            }
            else if (s.Value.bestNextChoice != null)
            {
                wrong[s.Value.bestNextChoice] = new KeyValuePair<int, int>(wrongCurrent.Key, wrongCurrent.Value + 1);
            }
        }

        private void AddCorrectPredictionResult(List<Node> partialCorrects, ref int correct, ref int partialCorrect, KeyValuePair<Node, PredictionResult> s, KeyValuePair<int, int> guessCurrent, KeyValuePair<int, int> partialCurrent, KeyValuePair<int, int> wrongCurrent)
        {
            if (partialCorrects.Contains(s.Value.bestNextChoice))
            {
                partials[s.Value.bestNextChoice] = new KeyValuePair<int, int>(partialCurrent.Key - 1, partialCurrent.Value - 1);
                partialCorrect--;
            }
            correct++;
            success[s.Value.bestNextChoice] = new KeyValuePair<int, int>(guessCurrent.Key + 1, guessCurrent.Value + 1);
            wrong[s.Value.bestNextChoice] = new KeyValuePair<int, int>(wrongCurrent.Key, wrongCurrent.Value + 1);
        }

       

        private bool updating = false;
        int repFps = 0;
        private void Repaint(object nothing)
        {
            if (repFps++ < 5)
            {
                return;
            }
            repFps = 0;
            if (updating)
            {
                return;
            }
            else
            {
                updating = true;
            }
            this.Dispatcher.BeginInvoke(new Action(UpdateProgress));
            lock (progressbars)
            {
                var ordered = this.progressbars.OrderByDescending(x => x.score).ToList();
                for (int i = 0; i < ordered.Count; i++)
                {
                    ProgressView v = ordered[i];
                    this.Dispatcher.BeginInvoke(new Func<int, bool>(v.ChangePos), i);
                    this.Dispatcher.BeginInvoke(new Action(v.Update));
                }
            }
            updating = false;

        }

        private void UpdateProgress()
        {

            double avgcorrect = 0;
            try
            {
                avgcorrect = ((double)success.Sum(x => x.Value.Key) / (double)success.Sum(x => x.Value.Value));
            }
            catch { }
            double avgpartial = 0;
            try
            {
                avgpartial = ((double)partials.Sum(x => x.Value.Key) / (double)partials.Sum(x => x.Value.Value));
            }
            catch { }

            double avgfailed = 0;
            try
            {
                avgfailed = ((double)wrong.Sum(x => x.Value.Key) / (double)wrong.Sum(x => x.Value.Value));
            }
            catch { }

            double missed = 1 - avgcorrect - avgfailed - avgpartial;

            ProgressLbl.Content = "Progress:" + completed + "/" + total;
            CorrectNumLbl.Content = ((int)(avgcorrect * 100)).ToString() + "%";
            PartialNumLbl.Content = ((int)(avgpartial * 100)).ToString() + "%";
            WrongNumLbl.Content = ((int)(avgfailed * 100)).ToString() + "%";
            MissingNumLbl.Content = ((int)(missed * 100)).ToString() + "%";
        }

        private class ProgressView
        {
            Dictionary<Node, KeyValuePair<int, int>> scores = new Dictionary<Node, KeyValuePair<int, int>>();
            Dictionary<Node, KeyValuePair<int, int>> wrongscores = new Dictionary<Node, KeyValuePair<int, int>>();
            Dictionary<Node, KeyValuePair<int, int>> partialScores = new Dictionary<Node, KeyValuePair<int, int>>();
            Node node;
            int top = 0;
            Canvas paintCanvas;
            Rectangle endLineRect = new Rectangle()
            {
                Height = 12,
                Width = 202,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Rectangle scoreRect = new Rectangle()
            {
                Fill = Brushes.Green,
                Stroke = Brushes.Green,
                StrokeThickness = 1,
                Height = 3
            };
            Rectangle wrongRect = new Rectangle()
            {
                Fill = Brushes.Red,
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                Height = 3
            };
            Rectangle partialRect = new Rectangle()
            {
                Fill = Brushes.Blue,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                Height = 3
            };

            Rectangle correctShowRect = new Rectangle { Fill = Brushes.Green, Width = 2, Height = 10 };
            Label correctShowLbl = new Label();
            Rectangle partialShowRect = new Rectangle() { Fill = Brushes.Blue, Width = 2, Height = 10 };
            Label partialShowLbl = new Label();
            Rectangle wrongShowRect = new Rectangle() { Fill = Brushes.Red, Width = 2, Height = 10 };
            Label wrongShowLbl = new Label();
            Label totLbl = new Label();
            Label name = new Label();

            public double score
            {
                get { return ((double)(scores[node].Key) / scores[node].Value) + ((double)(partialScores[node].Key) / partialScores[node].Value) / 10; }
            }

            public ProgressView(Node nd, Dictionary<Node, KeyValuePair<int, int>> Scores, Dictionary<Node, KeyValuePair<int, int>> wrongScores, Dictionary<Node, KeyValuePair<int, int>> partialScores, int idx, Canvas cnv)
            {
                this.scores = Scores;
                this.wrongscores = wrongScores;
                this.partialScores = partialScores;
                this.node = nd;
                this.top = 15 * idx + 30;
                this.paintCanvas = cnv;
                cnv.Children.Add(endLineRect);
                cnv.Children.Add(scoreRect);
                cnv.Children.Add(wrongRect);
                cnv.Children.Add(partialRect);
                cnv.Children.Add(name);
                cnv.Children.Add(correctShowRect);
                cnv.Children.Add(correctShowLbl);
                cnv.Children.Add(partialShowRect);
                cnv.Children.Add(partialShowLbl);
                cnv.Children.Add(wrongShowRect);
                cnv.Children.Add(wrongShowLbl);
                cnv.Children.Add(totLbl);

                var score = scores[node];
                double prob = ((double)score.Key / (double)score.Value);
                var wrongscore = wrongscores[node];
                var partialScore = partialScores[node];
                double wp = (((double)wrongscore.Key) / (double)score.Value);
                if (wp > 1)
                {
                    wp = 1;
                }
                double pp = (((double)partialScore.Key) / (double)partialScore.Value);
                name.Width = 600;
                name.Content = node.name;


                scoreRect.Width = 1 + 199 * (double.IsNaN(prob) || double.IsInfinity(prob) ? 0 : prob);
                wrongRect.Width = 1 + 199 * (double.IsNaN(wp) || double.IsInfinity(wp) ? 0 : wp);
                partialRect.Width = 1 + 199 * (double.IsNaN(pp) || double.IsInfinity(pp) ? 0 : pp);

                correctShowLbl.Content = Math.Round(double.IsNaN(prob) || double.IsInfinity(prob) ? 0 : prob * 100, 0) + "%";
                partialShowLbl.Content = Math.Round(double.IsNaN(pp) || double.IsInfinity(pp) ? 0 : pp * 100, 0) + "%";
                wrongShowLbl.Content = Math.Round(double.IsNaN(wp) || double.IsInfinity(wp) ? 0 : wp * 100, 0) + "%|" + score.Value;


                Canvas.SetTop(scoreRect, top + 10);
                Canvas.SetLeft(scoreRect, 3);
                Canvas.SetTop(partialRect, top + 13);
                Canvas.SetLeft(partialRect, 3);
                Canvas.SetTop(wrongRect, top + 16);
                Canvas.SetLeft(wrongRect, 3);
                Canvas.SetTop(name, top);
                Canvas.SetLeft(name, 335);
                Canvas.SetLeft(endLineRect, 2);
                Canvas.SetTop(endLineRect, top + 9);
                Canvas.SetTop(correctShowRect, top);
                Canvas.SetTop(correctShowLbl, top);
                Canvas.SetTop(partialShowRect, top);
                Canvas.SetTop(partialShowLbl, top);
                Canvas.SetTop(wrongShowRect, top);
                Canvas.SetTop(wrongShowLbl, top);
                Canvas.SetLeft(correctShowRect, 207);
                Canvas.SetLeft(correctShowLbl, 205);
                Canvas.SetLeft(partialShowRect, 241);
                Canvas.SetLeft(partialShowLbl, 239);
                Canvas.SetLeft(wrongShowRect, 277);
                Canvas.SetLeft(wrongShowLbl, 275);
                Canvas.SetLeft(totLbl, 305);
                Canvas.SetTop(totLbl, top);
            }

            public void Update()
            {

                var score = scores[node];
                double prob = ((double)score.Key / (double)score.Value);
                var wrongscore = wrongscores[node];
                var partialScore = partialScores[node];
                double wp = (((double)wrongscore.Key) / (double)score.Value);
                if (wp > 1)
                {
                    wp = 1;
                }
                double pp = (((double)partialScore.Key) / (double)partialScore.Value);


                scoreRect.Width = 1 + 199 * (double.IsNaN(prob) || double.IsInfinity(prob) || prob < 0 ? 0 : prob);
                wrongRect.Width = 1 + 199 * (double.IsNaN(wp) || double.IsInfinity(wp) || wp < 0 ? 0 : wp);
                partialRect.Width = 1 + 199 * (double.IsNaN(pp) || double.IsInfinity(pp) || pp < 0 ? 0 : pp);


                correctShowLbl.Content = Math.Round(double.IsNaN(prob) || double.IsInfinity(prob) ? 0 : prob * 100, 0) + "%";
                partialShowLbl.Content = Math.Round(double.IsNaN(pp) || double.IsInfinity(pp) ? 0 : pp * 100, 0) + "%";
                wrongShowLbl.Content = Math.Round(double.IsNaN(wp) || double.IsInfinity(wp) ? 0 : wp * 100, 0) + "%";
                totLbl.Content = score.Value;



            }

            public bool ChangePos(int pos)
            {
                this.top = 15 * pos + 15;
                Canvas.SetTop(scoreRect, top + 10);
                Canvas.SetTop(partialRect, top + 13);
                Canvas.SetTop(wrongRect, top + 16);
                //Canvas.SetTop(progress, top);
                //Canvas.SetTop(percent, top);
                Canvas.SetTop(name, top);
                Canvas.SetTop(endLineRect, top + 9);
                Canvas.SetTop(correctShowRect, top + 9);
                Canvas.SetTop(correctShowLbl, top);
                //Canvas.SetTop(correctSep, top+9);
                Canvas.SetTop(partialShowRect, top + 9);
                Canvas.SetTop(partialShowLbl, top);
                //Canvas.SetTop(partialSep, top+9);
                Canvas.SetTop(wrongShowRect, top + 9);
                Canvas.SetTop(wrongShowLbl, top);
                // Canvas.SetTop(wrongSep, top+9);
                Canvas.SetTop(totLbl, top);

                return true;
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tmr.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            tmr.Dispose();
        }

        

    }



}
