using System;
using GMTK;
using JetBrains.Annotations;
using UnityEngine;

public struct GameInput
{
	public const Buttons AnyDir = (Buttons.Down | Buttons.Left | Buttons.Up | Buttons.Right);
	public static GameInput None = new GameInput()
	{
		ArrowButton = null,
		JumpButton = null,
	};
	[CanBeNull] public ButtonEvent JumpButton;
	[CanBeNull] public ButtonEvent ArrowButton;
	public bool Any()
	{
		return (JumpButton != null) || (ArrowButton != null);
	}

	public Vector2 GetDir()
	{
		if (ArrowButton == null || (ArrowButton.Button & AnyDir) == 0)
		{
			return Vector2.zero;
		}
		//var h = (ArrowButton.Button & Buttons.Right) >0 ? 1 : 0 + (ArrowButton.Button & Buttons.Left) > 0 ? -1 : 0;
		//var v = (ArrowButton.Button & Buttons.Up) > 0 ? 1 : 0 + (ArrowButton.Button & Buttons.Down) > 0 ? -1 : 0;
		var h = 0;
		var v = 0;
		h += ArrowButton.Button == Buttons.Left ? -1 : 0;
		h += ArrowButton.Button == Buttons.Right ? 1 : 0;
		v += ArrowButton.Button == Buttons.Down ? -1 : 0;
		v += ArrowButton.Button == Buttons.Up ? 1 : 0;

		var v2 = new Vector2(h, v).normalized;
		
		return v2;
	}
}

[Flags]
public enum Buttons
{
	None = 0, 
	Jump = 1,
	Up = 2,
	Left = 4,
	Right = 8,
	Down = 16,
}