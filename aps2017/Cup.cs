using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AggressivePanhandlingSimulator2017
{
	public class Cup
	{
		public Button Button { get; set; }
		public Storyboard Storyboard { get; set; }

		public Cup(Button button)
		{
			Button = button;
		}
	}
}
