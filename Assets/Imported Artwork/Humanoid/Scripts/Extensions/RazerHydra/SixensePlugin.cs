using System.Runtime.InteropServices;
	
public partial class SixensePlugin
{
	[StructLayout( LayoutKind.Sequential )]
	public struct sixenseControllerData
	{
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 )]
		public float[] pos;
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 9 )]
		public float[] rot_mat;
		public float joystick_x;
		public float joystick_y;
		public float trigger;
		public uint buttons;
		public byte sequence_number;
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
		public float[] rot_quat;
		public ushort firmware_revision;
		public ushort hardware_revision;
		public ushort packet_type;
		public ushort magnetic_frequency;
		public int enabled;
		public int controller_index;
		public byte is_docked;
		public byte which_hand;
		public byte hemi_tracking_enabled;
	}
	
	[StructLayout( LayoutKind.Sequential )]
	public struct sixenseAllControllerData
	{
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
		public sixenseControllerData[] controllers;
	}
	
	[DllImport( "sixense", EntryPoint = "sixenseInit" )]
	public static extern int sixenseInit();
	
	[DllImport( "sixense", EntryPoint = "sixenseExit" )]
	public static extern int sixenseExit();
	
	[DllImport( "sixense", EntryPoint = "sixenseGetMaxBases" )]
	public static extern int sixenseGetMaxBases();
	
	[DllImport( "sixense", EntryPoint = "sixenseSetActiveBase" )]
	public static extern int sixenseSetActiveBase( int i );
	
	[DllImport( "sixense", EntryPoint = "sixenseIsBaseConnected" )]
	public static extern int sixenseIsBaseConnected( int i );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetMaxControllers" )]
	public static extern int sixenseGetMaxControllers();
	
	[DllImport( "sixense", EntryPoint = "sixenseIsControllerEnabled" )]
	public static extern int sixenseIsControllerEnabled( int which );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetNumActiveControllers" )]
	public static extern int sixenseGetNumActiveControllers();
	
	[DllImport( "sixense", EntryPoint = "sixenseGetHistorySize" )]
	public static extern int sixenseGetHistorySize();
	
	[DllImport( "sixense", EntryPoint = "sixenseGetData" )]
	public static extern int sixenseGetData( int which, int index_back, ref sixenseControllerData cd );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetAllData" )]
	public static extern int sixenseGetAllData( int index_back, ref sixenseAllControllerData acd );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetNewestData" )]
	public static extern int sixenseGetNewestData( int which, ref sixenseControllerData cd );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetAllNewestData" )]
	public static extern int sixenseGetAllNewestData( ref sixenseAllControllerData acd );
	
	[DllImport( "sixense", EntryPoint = "sixenseSetHemisphereTrackingMode" )]
	public static extern int sixenseSetHemisphereTrackingMode( int which_controller, int state );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetHemisphereTrackingMode" )]
	public static extern int sixenseGetHemisphereTrackingMode( int which_controller, ref int state );
	
	[DllImport( "sixense", EntryPoint = "sixenseAutoEnableHemisphereTracking" )]
	public static extern int sixenseAutoEnableHemisphereTracking( int which_controller );
	
	[DllImport( "sixense", EntryPoint = "sixenseSetHighPriorityBindingEnabled" )]
	public static extern int sixenseSetHighPriorityBindingEnabled( int on_or_off );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetHighPriorityBindingEnabled" )]
	public static extern int sixenseGetHighPriorityBindingEnabled( ref int on_or_off );
	
	[DllImport( "sixense", EntryPoint = "sixenseTriggerVibration" )]
	public static extern int sixenseTriggerVibration( int controller_id, int duration_100ms, int pattern_id );
	
	[DllImport( "sixense", EntryPoint = "sixenseSetFilterEnabled" )]
	public static extern int sixenseSetFilterEnabled( int on_or_off );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetFilterEnabled" )]
	public static extern int sixenseGetFilterEnabled( ref int on_or_off );
	
	[DllImport( "sixense", EntryPoint = "sixenseSetFilterParams" )]
	public static extern int sixenseSetFilterParams( float near_range, float near_val, float far_range, float far_val );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetFilterParams" )]
	public static extern int sixenseGetFilterParams( ref float near_range, ref float near_val, ref float far_range, ref float far_val );
	
	[DllImport( "sixense", EntryPoint = "sixenseSetBaseColor" )]
	public static extern int sixenseSetBaseColor( byte red, byte green, byte blue );
	
	[DllImport( "sixense", EntryPoint = "sixenseGetBaseColor" )]
	public static extern int sixenseGetBaseColor( ref byte red, ref byte green, ref byte blue );
}
