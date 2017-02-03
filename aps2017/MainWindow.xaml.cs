using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AggressivePanhandlingSimulator2017
{
	public partial class MainWindow : Window
	{
		private Random rng;
		private int winningCup;
		private Cup[] cups;
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

		private const uint MaxWins = 4;

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
			cups = new Cup[]
			{
				new Cup(Cup1), new Cup(Cup2), new Cup(Cup3)
			};

			var marginProperty = DependencyProperty.Register("Margin", typeof(Thickness), typeof(Button));
			foreach (var cup in cups)
			{
				var animationFrames = new ThicknessKeyFrameCollection();
				animationFrames.Add(new SplineThicknessKeyFrame(new Thickness(32, 20, 32, 108), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2))));
				animationFrames.Add(new SplineThicknessKeyFrame(new Thickness(32, 20, 32, 108), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.8))));
				animationFrames.Add(new SplineThicknessKeyFrame(new Thickness(32, 64, 32, 64), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))));
				var animation = new ThicknessAnimationUsingKeyFrames();
				animation.KeyFrames = animationFrames;
				cup.Storyboard = new Storyboard();
				cup.Storyboard.Children.Add(animation);
				Storyboard.SetTargetName(animation, cup.Button.Name);
				Storyboard.SetTargetProperty(animation, new PropertyPath(Button.MarginProperty));
			}

			gameTransition = false;
			streak = 0;
			NewGame(false);
		}

		public void NewGame(bool wonlasttime)
		{
			if (streak >= MaxWins)
			{
				InfoLabel.Text = "Cheater.";
				RemoveCups();
			}
			else
			{ 
				InfoLabel.Text = wonlasttime ? GamePromptAfterWin : GamePrompt;
				for (int i = 0; i < cups.Length; i++)
				{
					cups[i].Button.Background = CupDefaultColor;
					cups[i].Button.Foreground = AppForegroundColor;
					cups[i].Button.BorderBrush = AppForegroundColor;
				}

				winningCup = rng.Next(3);
			}
		}

		private void Cup1_Clicked(object sender, RoutedEventArgs e)
		{
			Play(0);
		}

		private void Cup2_Clicked(object sender, RoutedEventArgs e)
		{
			Play(1);
		}

		private void Cup3_Clicked(object sender, RoutedEventArgs e)
		{
			Play(2);
		}

		private void Play(uint cup)
		{
			bool win = cup == winningCup;
			
			if (gameTransition) // ignore buttons if game has just been played
				return;

			cups[cup].Storyboard.Begin(this);

			if (win)
				streak++;
			else
				streak = 0;
			
			RevealCups();
			InfoLabel.Text = (win ? GameWinPrompt : GameLossPrompt);

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
					cups[i].Button.Background = CupWinColor;
					cups[i].Button.Foreground = CupWinDarkColor;
					cups[i].Button.BorderBrush = CupWinDarkColor;
				}
				else
				{
					cups[i].Button.Background = CupLoseColor;
					cups[i].Button.Foreground = CupLoseDarkColor;
					cups[i].Button.BorderBrush = CupLoseDarkColor;
				}
			}
		}

		private void RemoveCups()
		{
			foreach (var cup in cups)
				cup.Button.Visibility = Visibility.Hidden;
			StreakLabel.Visibility = Visibility.Hidden;
			BestStreakLabel.Visibility = Visibility.Hidden;
		}

        private void GameplayTimedThread(object win)
        {
            gameThreadUpdateOneArg g = NewGame;
            Thread.Sleep(TimeSpan.FromSeconds((streak >= MaxWins ? 1 : 2)));
            gameTransition = false;
            try
            { 
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, g, win);
            }
            catch(NullReferenceException)
            {
                // don't know the proper thing to do, but this only happens
                // on program exit when the window is closed but this thread 
                // is still running... So I'm just ignoring them right now.
            }
        }
	}
}
