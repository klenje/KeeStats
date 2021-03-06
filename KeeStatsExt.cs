/*
  KeeStats - A plugin for Keepass Password Manager
  Copyright (C) 2014 Andrea Decorte

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using KeePass.Plugins;

using KeePassLib;

namespace KeeStats
{
	/// <summary>
	/// This is the main plugin class
	/// </summary>
	public sealed class KeeStatsExt : Plugin
	{
		private IPluginHost m_host = null;

		// Menus
		private ToolStripSeparator m_tsSeparator = null;
		private ToolStripSeparator m_tsSeparator2 = null;
		private ToolStripMenuItem m_tsmiStats = null;
		private ToolStripMenuItem m_GroupStats = null;
		
		/// <summary>
		/// The <c>Initialize</c> function is called by KeePass when
		/// you should initialize your plugin (create menu items, etc.).
		/// </summary>
		/// <param name="host">Plugin host interface. By using this
		/// interface, you can access the KeePass main window and the
		/// currently opened database.</param>
		/// <returns>You must return <c>true</c> in order to signal
		/// successful initialization. If you return <c>false</c>,
		/// KeePass unloads your plugin (without calling the
		/// <c>Terminate</c> function of your plugin).</returns>
		public override bool Initialize(IPluginHost host)
		{
			Debug.Assert(host != null);
			if(host == null) return false;
			m_host = host;
			
			// Get a reference to the 'Tools' menu item container
			ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;

			// Add a separator at the bottom
			m_tsSeparator = new ToolStripSeparator();
			tsMenu.Add(m_tsSeparator);
			
			m_tsmiStats = new ToolStripMenuItem();
			m_tsmiStats.Text = "&View stats...";
			tsMenu.Add(m_tsmiStats);
			m_tsmiStats.Click += OnMenuViewStats;
			
			// Add a seperator and menu item to the group context menu
			ContextMenuStrip contextMenu = m_host.MainWindow.GroupContextMenu;
			m_tsSeparator2 = new ToolStripSeparator();
			contextMenu.Items.Add(m_tsSeparator2);
			m_GroupStats = new ToolStripMenuItem();
			m_GroupStats.Text = "View stats for this group...";
			m_GroupStats.Click += OnMenuGroupStats;
			contextMenu.Items.Add(m_GroupStats);

			return true; // Initialization successful
		}

		/// <summary>
		/// The <c>Terminate</c> function is called by KeePass when
		/// you should free all resources, close open files/streams,
		/// etc. It is also recommended that you remove all your
		/// plugin menu items from the KeePass menu.
		/// </summary>
		public override void Terminate()
		{
			// Remove all of our menu items
			ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;
			tsMenu.Remove(m_tsSeparator);
			tsMenu.Remove(m_tsmiStats);
			
			// Remove group context menu items
			ContextMenuStrip contextMenu = m_host.MainWindow.GroupContextMenu;
			contextMenu.Items.Remove(m_tsSeparator2);
			contextMenu.Items.Remove(m_GroupStats);
		}
		
		private void OnMenuViewStats(object sender, EventArgs e)
		{
			if(!m_host.Database.IsOpen)
			{
				MessageBox.Show("You first need to open a database!", "KeeStats");
				return;
			}
			
			ComputeStats(m_host.Database.RootGroup);
		}
		
		private void OnMenuGroupStats(object sender, EventArgs e)
		{
			if(!m_host.Database.IsOpen)
			{
				MessageBox.Show("You first need to open a database!", "KeeStats");
				return;
			}
			
			PwGroup theGroup = m_host.MainWindow.GetSelectedGroup();
			Debug.Assert(theGroup != null); if (theGroup == null) return;
			ComputeStats(theGroup);
		}

		/// <summary>
		/// Computes the statistics
		/// </summary>
		/// <param name="group">The group on which to calculate the statistics</param>
		private void ComputeStats(PwGroup group)
		{
			List<StatItem> statsList = new List<StatItem>();
			List<ExtendedStatItem> extended_stats = new List<ExtendedStatItem>();
			
			// By default is recursive
			if (!StatComputer.ComputeStats(group, ref statsList, ref extended_stats, true)) {
				// exit if no password
				MessageBox.Show("No passwords in this group", "KeeStats", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			
			// Show the window
			StatsSummaryWindow theWindow = new StatsSummaryWindow(statsList, extended_stats);
			theWindow.Database = m_host.Database;
			theWindow.Group = group;
			// We need the icons for the Edit Entry Form
			theWindow.Icons = m_host.MainWindow.ClientIcons;
			theWindow.Show();
		}

		// Set this link to where the version update file is located
		public override string UpdateUrl
		{
			get
			{
				return "https://github.com/klenje/KeeStats/blob/master/version.txt";
			}
		}
	}
}
