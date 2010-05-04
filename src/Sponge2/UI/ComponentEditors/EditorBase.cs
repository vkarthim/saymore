// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2010, SIL International. All Rights Reserved.
// <copyright from='2010' to='2010' company='SIL International'>
//		Copyright (c) 2010, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: EditorBase.cs
// Responsibility: D. Olson
//
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Windows.Forms;
using Sponge2.UI.Utilities;

namespace Sponge2.UI.ComponentEditors
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class EditorBase : UserControl
	{
		/// ------------------------------------------------------------------------------------
		public EditorBase()
		{
			DoubleBuffered = true;
			BackColor = SpongeColors.DataEntryPanelBegin;
			Padding = new Padding(7);
			AutoScroll = true;
		}
	}
}