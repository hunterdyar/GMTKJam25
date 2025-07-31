using System;
using GMTK;
using JetBrains.Annotations;
using UnityEngine;

public struct GameInput
{
	public const Buttons AnyDir = (Buttons.Down | Buttons.Left | Buttons.Up | Buttons.Right);
	public static GameInput None = new GameInput();
	[CanBeNull] public ButtonEvent JumpButton;
	[CanBeNull] public ButtonEvent ArrowButton;
	public bool Any()
	{
		return JumpButton != null || ArrowButton != null;
	}

	public Vector2 GetDir()
	{
		if (ArrowButton == null || (ArrowButton.Button & AnyDir) == 0)
		{
			return Vector2.zero;
		}
		var h = (ArrowButton.Button & Buttons.Right) >0 ? 1 : 0 + (ArrowButton.Button & Buttons.Left) > 0 ? -1 : 0;
		var v = (ArrowButton.Button & Buttons.Up) > 0 ? 1 : 0 + (ArrowButton.Button & Buttons.Down) > 0 ? -1 : 0;
		return new Vector2(h, v).normalized;
	}
}

[Flags]
public enum Buttons
{
	Jump,
	Up,
	Left,
	Right,
	Down
}