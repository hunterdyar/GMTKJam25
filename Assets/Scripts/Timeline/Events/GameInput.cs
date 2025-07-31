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
	[CanBeNull] public ButtonEvent ArrowButtonB;

	public bool Any()
	{
		return (JumpButton != null) || (ArrowButton != null);
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
	UpRight = Up | Right,
	UpLeft = Up | Left,
	DownLeft = Down | Left,
	DownRight = Down | Right,
}