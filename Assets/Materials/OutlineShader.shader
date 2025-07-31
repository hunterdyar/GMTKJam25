Shader "Custom/OutlineSolidOffset"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Front
        ZWrite On
        ZTest LEqual
        Offset -1, -1 // helps prevent z-fighting

        Pass
        {
            Color (0,0,0,1)
        }
    }
}
