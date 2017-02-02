using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AggressivePanhandlingSimulator2017
{
	public partial class MainWindow : Window
	{
		private Random rng;
		private int winningCup;
		private Button[] cups;
		private uint streak;
		private uint maxStreak;

		public SolidColorBrush AppForegroundColor;
		public SolidColorBrush AppBackgroundColor;
		public SolidColorBrush CupWinColor;
		public SolidColorBrush CupWinDarkColor;
		public SolidColorBrush CupLoseColor;
		public SolidColorBrush CupLoseDarkColor;
		public SolidColorBrush CupDefaultColor;
		
		public volatile bool gameTransition;
        private delegate void gameThreadUpdateOneArg(bool arg);
        private delegate void gameThreadUpdateNoArg();

        private const string GamePrompt = "Pick a cup.";
		private const string GamePromptAfterWin = "Push your luck.";
		private const string GameWinPrompt = "Nice.";
		private const string GameLossPrompt = "Bummer.";

		public MainWindow()
		{
			InitializeComponent();

			AppForegroundColor = new SolidColorBrush((Color)FindResource("AppForegroundColor"));
			AppBackgroundColor = new SolidColorBrush((Color)FindResource("AppBackgroundColor"));
			CupWinColor		   = new SolidColorBrush((Color)FindResource("CupWinColor"));
			CupWinDarkColor    = new SolidColorBrush((Color)FindResource("CupWinDarkColor"));
			CupLoseColor       = new SolidColorBrush((Color)FindResource("CupLoseColor"));
			CupLoseDarkColor   = new SolidColorBrush((Color)FindResource("CupLoseDarkColor"));
			CupDefaultColor    = new SolidColorBrush((Color)FindResource("CupDefaultColor"));

			rng = new Random();
			cups = new Button[] { Cup1, Cup2, Cup3 };

			gameTransition = false;
			streak = 0;
			NewGame(false);
		}

		public void NewGame(bool wonlasttime)
		{
			InfoLabel.Text = wonlasttime ? GamePromptAfterWin : GamePrompt;

			for (int i = 0; i < cups.Length; i++)
			{
				cups[i].Background = CupDefaultColor;
				cups[i].Foreground = AppForegroundColor;
				cups[i].BorderBrush = AppForegroundColor;
			}

			winningCup = rng.Next(3);
		}
		
		private void Cup1_Clicked(object sender, RoutedEventArgs e)
		{
			Play(winningCup == 0);
		}

		private void Cup2_Clicked(object sender, RoutedEventArgs e)
		{
			Play(winningCup == 1);
		}

		private void Cup3_Clicked(object sender, RoutedEventArgs e)
		{
			Play(winningCup == 2);
		}

		private void Play(bool win)
		{
			if (gameTransition) // ignore buttons if game has just been played
				return;

			// UI updates to show the play
			RevealCups();
			InfoLabel.Text = (win ? GameWinPrompt : GameLossPrompt);
			if (win)
				streak++;
			else
				streak = 0;

			maxStreak = Math.Max(streak, maxStreak);

			StreakLabel.Text = "Win Streak: " + streak;
			BestStreakLabel.Text = "Best: " + maxStreak;

			gameTransition = true;

            // force a render of the UI
            gameThreadUpdateNoArg g = () => { };
			Application.Current.Dispatcher.Invoke( DispatcherPriority.Render, g);

			// start a task which will reset the UI after 2 seconds
			Thread newthread = new Thread(new ParameterizedThreadStart(GameplayTimedThread));
            newthread.Start();
		}

		private void RevealCups()
		{
			for (int i = 0; i < cups.Length; i++)
			{
				if (i == winningCup)
				{ 
					cups[i].Background = CupWinColor;
					cups[i].Foreground = CupWinDarkColor;
					cups[i].BorderBrush = CupWinDarkColor;
				}
				else
				{
					cups[i].Background = CupLoseColor;
					cups[i].Foreground = CupLoseDarkColor;
					cups[i].BorderBrush = CupLoseDarkColor;
				}
			}
		}

        private void GameplayTimedThread(object win)
        {
            gameThreadUpdateOneArg g = NewGame;
            Thread.Sleep(TimeSpan.FromSeconds(2));
            gameTransition = false;
            try
            { 
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, g, win);
            }
            catch(NullReferenceException)
            {
                // ignore nullreferenceexceptions...

                // don't know the proper thing to do, but this only happens
                // on program exit when the window is closed but this thread 
                // is still running...
            }
        }
	}
}
