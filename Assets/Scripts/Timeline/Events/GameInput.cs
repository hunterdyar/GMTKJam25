using System;
using GMTK;
using JetBrains.Annotations;

public struct GameInput
{
	public static GameInput None = new GameInput();
	[CanBeNull] public ButtonEvent JumpButton;
	
	public bool Any()
	{
		return JumpButton != null;
	}
}

public enum Buttons
{
	Jump
}