using DBPSim.Events;
using DBPSim.Simulation;
using DBPSim.Simulation.Simulator;
using DBPSim.SimulationGUI.ViewModels;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace DBPSim.SimulationGUI.Tabs
{
    partial class TabItemSimulation
    {

        private Thread _simulationThread;
        private int _threadPauseMsTime = 50; // Default


        private void ButtonSimulationStart_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateSimulationCanBeStarted())
            {
                return;
            }

            if (SimulatorHelper.IsInited())
            {
                this.InitSimulationTime();
                SimulatorHelper.RestartSimulation();
                SimulatorHelper.InitSimulator();
                SimulationLogViewModel.WriteLineWithDate("Simulation started");
            }
            else
            {
                MessageBox.Show("Simulator was inited before. If you want to restart, please stop simulation or press restart button.");
            }
        }


        private void ButtonFastSimulationStart_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateSimulationCanBeStarted())
            {
                return;
            }

            //this.ButtonSimulationStart.IsEnabled = false;
            //this.ButtonSimulationRestart.IsEnabled = false;
            //this.ButtonSimulationPreviousStep.IsEnabled = false;
            //this.ButtonSimulationNextStep.IsEnabled = false;

            SimulationLogViewModel.WriteLineWithDate("Simulation started");

            if (SimulatorHelper.IsStarted())
            {
                this.InitSimulationTime();
                SimulatorHelper.RestartSimulation();
                SimulatorHelper.InitSimulator();
            }

            if (this._simulationThread == null || this._simulationThread.ThreadState == ThreadState.Stopped)
            {
                this._simulationThread = new Thread(new ThreadStart(DoFastSimulation));
                this._simulationThread.SetApartmentState(ApartmentState.STA);
                this._simulationThread.Start();
            }
        }


        private void ButtonSimulationPause_Click(object sender, RoutedEventArgs e)
        {
            this.ButtonSimulationStart.IsEnabled = true;
            this.ButtonSimulationRestart.IsEnabled = true;
            this.ButtonSimulationPreviousStep.IsEnabled = true;
            this.ButtonSimulationNextStep.IsEnabled = true;

            if (_simulationThread != null)
            {
                _simulationThread.Abort();
                _simulationThread = null;
            }
        }


        private void ButtonSimulationRestart_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you really want to restart simulation?",
                         "Validation",
                         System.Windows.MessageBoxButton.OKCancel,
                         System.Windows.MessageBoxImage.Question,
                         System.Windows.MessageBoxResult.Cancel) == System.Windows.MessageBoxResult.OK)
            {
                SimulatorHelper.RestartSimulation();

                // Initialization
                ProcessModelInstancesViewModel.Init();
                SimulationWatchPointViewModel.Init();
                EventsViewModel.Init();
                this.ClearSimulationModel();
                this.ReDrawSimulationModel();
            }
        }


        private void ButtonSimulationPreviousStep_Click(object sender, RoutedEventArgs e)
        {
            SimulatorHelper.PreviousStep(SimulatorHelper.GetCurrentlyModeledInstance().ProcessInstance.Number);
            SimulationWatchPointViewModel.Init();
            this.ReDrawSimulationModel();
        }


        private void ButtonSimulationNextStep_Click(object sender, RoutedEventArgs e)
        {
            if (!this.ValidateSimulationCanBeStarted())
            {
                return;
            }

            if (SimulatorHelper.GetCurrentlyModeledInstance().Status == SimulatorStatus.Finished ||
                SimulatorHelper.GetCurrentlyModeledInstance().Status == SimulatorStatus.Unknown ||
                SimulatorHelper.GetCurrentlyModeledInstance().Status == SimulatorStatus.Error)
            {
            }
            else
            {
                bool finished = false;
                SimulatorHelper.NextStep(out finished);
            }

            // initialization
            RuleExecutionResultViewModel.Init();
            EventsViewModel.Init();
            this.ReDrawSimulationModel();
        }


        private void DoFastSimulation()
        {
            bool finished = false;
            while (!finished)
            {
                SimulatorHelper.NextStep(out finished);

                RuleExecutionResultViewModel.Init();
                EventsViewModel.Init();

                // Exception will be thrown here if uncommented.
                //this.ReDrawSimulationModel();
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.ReDrawSimulationModel();
                }));
                Thread.Sleep(this._threadPauseMsTime);
            }
        }


        private void InitSimulationTime()
        {
            Event firstEvent = Context.Events.GetFirstProcessingEvent();
            if (firstEvent.EventDate != null)
            {
                long time = (long)((firstEvent.EventDate.Value - new DateTime(1970, 1, 1)).TotalMinutes + 0.5);
                Context.ResetTime(time);
                SimulatorHelper.GetCurrentlyModeledInstance().SimulationTime = time;
            }
        }

    }
}
