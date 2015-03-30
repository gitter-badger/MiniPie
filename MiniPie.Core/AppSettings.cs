﻿using System;
using System.Collections.Generic;
using Caliburn.Micro;
using MiniPie.Core.Enums;

namespace MiniPie.Core {
	public sealed class AppSettings : PropertyChangedBase {

		public AppSettings() {
			Positions = new List<WindowPosition>();
			ReadBroadcastMessageIds = new List<string>();
			UniqueApplicationIdentifier = Guid.NewGuid().ToString();
			ApplicationSize = ApplicationSize.Medium;
		}

		private bool _AlwaysOnTop;
		public bool AlwaysOnTop {
			get { return _AlwaysOnTop; }
			set { _AlwaysOnTop = value; NotifyOfPropertyChange(); }
		}

		private bool _StartWithWindows;
		public bool StartWithWindows {
			get { return _StartWithWindows; }
			set { _StartWithWindows = value; NotifyOfPropertyChange(); }
		}

		private bool _HideIfSpotifyClosed;
		public bool HideIfSpotifyClosed {
			get { return _HideIfSpotifyClosed; }
			set { _HideIfSpotifyClosed = value; NotifyOfPropertyChange(); }
		}

		private bool _DisableAnimations;
		public bool DisableAnimations {
			get { return _DisableAnimations; }
			set { _DisableAnimations = value; NotifyOfPropertyChange(); }
		}

	    private Language _language;

	    public Language Language
	    {
	        get { return _language; }
	        set
	        {
	            _language = value;
	            NotifyOfPropertyChange();
	        }
	    }

		public List<WindowPosition> Positions { get; set; }
		public List<string> ReadBroadcastMessageIds { get; set; }
		public string UniqueApplicationIdentifier { get; set; }

		private ApplicationSize _ApplicationSize;
		public ApplicationSize ApplicationSize {
			get { return _ApplicationSize; }
			set { _ApplicationSize = value; NotifyOfPropertyChange(); }
		}

	    private bool _HotKeysEnabled;

	    public bool HotKeysEnabled
	    {
            get { return _HotKeysEnabled; }
            set { _HotKeysEnabled = value; NotifyOfPropertyChange(); }
	    }

	    private HotKeys _HotKeys;

	    public HotKeys HotKeys
	    {
            get { return _HotKeys; }
	        set
	        {
                _HotKeys = value; 
                NotifyOfPropertyChange();
	        }
	    }
	}
}