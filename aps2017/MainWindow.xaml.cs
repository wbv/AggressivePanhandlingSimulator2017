using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace WpfApplication1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public Random rng;
		public int winningCup;
		public Button[] cups;

		public SolidColorBrush fgColor;
		public SolidColorBrush bgColor;
		public SolidColorBrush winningCupColor;
		public SolidColorBrush wrongCupColor;
		public SolidColorBrush blankCupColor;
		
		public volatile bool gameTransition;
		private delegate void delegater(bool arg);

		private const string GamePrompt = "Pick a cup.";
		private const string GameWinPrompt = "Nice.";
		private const string GameLossPrompt = "Bummer.";

		public MainWindow()
		{
			InitializeComponent();

			fgColor = new SolidColorBrush(
			              new Color {
							  R = 106,
							  G = 120,
							  B = 132,
							  A = byte.MaxValue
						  }
			);

			bgColor = new SolidColorBrush(
						  new Color
						  {
							  R = 220,
							  G = 234,
							  B = 249,
							  A = byte.MaxValue
						  }
			);

			winningCupColor = new SolidColorBrush(
						  new Color
						  {
							  R = 34,
							  G = 177,
							  B = 76,
							  A = byte.MaxValue
						  }
			);

			wrongCupColor = new SolidColorBrush(
						  new Color
						  {
							  R = 149,
							  G = 64,
							  B = 64,
							  A = byte.MaxValue
						  }
			);

			blankCupColor = new SolidColorBrush(
						  new Color
						  {
							  R = 149,
							  G = 149,
							  B = 149,
							  A = byte.MaxValue
						  }
			);

			Foreground = fgColor;
			Background = bgColor;

			rng = new Random();
			cups = new Button[] { Cup1, Cup2, Cup3 };

			foreach (var cup in cups)
			{
				//cup.;
			}

			gameTransition = false;

			NewGame(false);
		}

		public void NewGame(bool wonlasttime)
		{
			InfoLabel.Text = GamePrompt;

			for (int i = 0; i < cups.Count(); i++)
				cups[i].Background = blankCupColor;
			//UpdateLayout();

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

			RevealCups();
			InfoLabel.Text = (win ? GameWinPrompt : GameLossPrompt);
			gameTransition = true;

			Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

			var startnextgame = new delegater(NewGame);

			Task.Run(() =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(2));
				gameTransition = false;
				Application.Current.Dispatcher.Invoke(startnextgame, win);
			});
		}

		private void RevealCups()
		{
			for (int i = 0; i < cups.Count(); i++)
			{
				if (i == winningCup)
					cups[i].Background = winningCupColor;
				else
					cups[i].Background = wrongCupColor;

			}
		}
	}
}
