using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0.15,0.15,0,0]")]
	public partial class CubeNetworkObject : NetworkObject
	{
		public const int IDENTITY = 4;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		private Vector3 _position;
		public event FieldEvent<Vector3> positionChanged;
		public InterpolateVector3 positionInterpolation = new InterpolateVector3() { LerpT = 0.15f, Enabled = true };
		public Vector3 position
		{
			get { return _position; }
			set
			{
				// Don't do anything if the value is the same
				if (_position == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_position = value;
				hasDirtyFields = true;
			}
		}

		public void SetpositionDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_position(ulong timestep)
		{
			if (positionChanged != null) positionChanged(_position, timestep);
			if (fieldAltered != null) fieldAltered("position", _position, timestep);
		}
		private Quaternion _rotation;
		public event FieldEvent<Quaternion> rotationChanged;
		public InterpolateQuaternion rotationInterpolation = new InterpolateQuaternion() { LerpT = 0.15f, Enabled = true };
		public Quaternion rotation
		{
			get { return _rotation; }
			set
			{
				// Don't do anything if the value is the same
				if (_rotation == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_rotation = value;
				hasDirtyFields = true;
			}
		}

		public void SetrotationDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_rotation(ulong timestep)
		{
			if (rotationChanged != null) rotationChanged(_rotation, timestep);
			if (fieldAltered != null) fieldAltered("rotation", _rotation, timestep);
		}
		private uint _ownerNetId;
		public event FieldEvent<uint> ownerNetIdChanged;
		public Interpolated<uint> ownerNetIdInterpolation = new Interpolated<uint>() { LerpT = 0f, Enabled = false };
		public uint ownerNetId
		{
			get { return _ownerNetId; }
			set
			{
				// Don't do anything if the value is the same
				if (_ownerNetId == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x4;
				_ownerNetId = value;
				hasDirtyFields = true;
			}
		}

		public void SetownerNetIdDirty()
		{
			_dirtyFields[0] |= 0x4;
			hasDirtyFields = true;
		}

		private void RunChange_ownerNetId(ulong timestep)
		{
			if (ownerNetIdChanged != null) ownerNetIdChanged(_ownerNetId, timestep);
			if (fieldAltered != null) fieldAltered("ownerNetId", _ownerNetId, timestep);
		}
		private int _colorId;
		public event FieldEvent<int> colorIdChanged;
		public Interpolated<int> colorIdInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int colorId
		{
			get { return _colorId; }
			set
			{
				// Don't do anything if the value is the same
				if (_colorId == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x8;
				_colorId = value;
				hasDirtyFields = true;
			}
		}

		public void SetcolorIdDirty()
		{
			_dirtyFields[0] |= 0x8;
			hasDirtyFields = true;
		}

		private void RunChange_colorId(ulong timestep)
		{
			if (colorIdChanged != null) colorIdChanged(_colorId, timestep);
			if (fieldAltered != null) fieldAltered("colorId", _colorId, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			positionInterpolation.current = positionInterpolation.target;
			rotationInterpolation.current = rotationInterpolation.target;
			ownerNetIdInterpolation.current = ownerNetIdInterpolation.target;
			colorIdInterpolation.current = colorIdInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _position);
			UnityObjectMapper.Instance.MapBytes(data, _rotation);
			UnityObjectMapper.Instance.MapBytes(data, _ownerNetId);
			UnityObjectMapper.Instance.MapBytes(data, _colorId);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_position = UnityObjectMapper.Instance.Map<Vector3>(payload);
			positionInterpolation.current = _position;
			positionInterpolation.target = _position;
			RunChange_position(timestep);
			_rotation = UnityObjectMapper.Instance.Map<Quaternion>(payload);
			rotationInterpolation.current = _rotation;
			rotationInterpolation.target = _rotation;
			RunChange_rotation(timestep);
			_ownerNetId = UnityObjectMapper.Instance.Map<uint>(payload);
			ownerNetIdInterpolation.current = _ownerNetId;
			ownerNetIdInterpolation.target = _ownerNetId;
			RunChange_ownerNetId(timestep);
			_colorId = UnityObjectMapper.Instance.Map<int>(payload);
			colorIdInterpolation.current = _colorId;
			colorIdInterpolation.target = _colorId;
			RunChange_colorId(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _position);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _rotation);
			if ((0x4 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _ownerNetId);
			if ((0x8 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _colorId);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (positionInterpolation.Enabled)
				{
					positionInterpolation.target = UnityObjectMapper.Instance.Map<Vector3>(data);
					positionInterpolation.Timestep = timestep;
				}
				else
				{
					_position = UnityObjectMapper.Instance.Map<Vector3>(data);
					RunChange_position(timestep);
				}
			}
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (rotationInterpolation.Enabled)
				{
					rotationInterpolation.target = UnityObjectMapper.Instance.Map<Quaternion>(data);
					rotationInterpolation.Timestep = timestep;
				}
				else
				{
					_rotation = UnityObjectMapper.Instance.Map<Quaternion>(data);
					RunChange_rotation(timestep);
				}
			}
			if ((0x4 & readDirtyFlags[0]) != 0)
			{
				if (ownerNetIdInterpolation.Enabled)
				{
					ownerNetIdInterpolation.target = UnityObjectMapper.Instance.Map<uint>(data);
					ownerNetIdInterpolation.Timestep = timestep;
				}
				else
				{
					_ownerNetId = UnityObjectMapper.Instance.Map<uint>(data);
					RunChange_ownerNetId(timestep);
				}
			}
			if ((0x8 & readDirtyFlags[0]) != 0)
			{
				if (colorIdInterpolation.Enabled)
				{
					colorIdInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					colorIdInterpolation.Timestep = timestep;
				}
				else
				{
					_colorId = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_colorId(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (positionInterpolation.Enabled && !positionInterpolation.current.UnityNear(positionInterpolation.target, 0.0015f))
			{
				_position = (Vector3)positionInterpolation.Interpolate();
				//RunChange_position(positionInterpolation.Timestep);
			}
			if (rotationInterpolation.Enabled && !rotationInterpolation.current.UnityNear(rotationInterpolation.target, 0.0015f))
			{
				_rotation = (Quaternion)rotationInterpolation.Interpolate();
				//RunChange_rotation(rotationInterpolation.Timestep);
			}
			if (ownerNetIdInterpolation.Enabled && !ownerNetIdInterpolation.current.UnityNear(ownerNetIdInterpolation.target, 0.0015f))
			{
				_ownerNetId = (uint)ownerNetIdInterpolation.Interpolate();
				//RunChange_ownerNetId(ownerNetIdInterpolation.Timestep);
			}
			if (colorIdInterpolation.Enabled && !colorIdInterpolation.current.UnityNear(colorIdInterpolation.target, 0.0015f))
			{
				_colorId = (int)colorIdInterpolation.Interpolate();
				//RunChange_colorId(colorIdInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public CubeNetworkObject() : base() { Initialize(); }
		public CubeNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public CubeNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
