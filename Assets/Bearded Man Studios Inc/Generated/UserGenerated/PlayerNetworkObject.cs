using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0,0]")]
	public partial class PlayerNetworkObject : NetworkObject
	{
		public const int IDENTITY = 8;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
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
				_dirtyFields[0] |= 0x1;
				_ownerNetId = value;
				hasDirtyFields = true;
			}
		}

		public void SetownerNetIdDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_ownerNetId(ulong timestep)
		{
			if (ownerNetIdChanged != null) ownerNetIdChanged(_ownerNetId, timestep);
			if (fieldAltered != null) fieldAltered("ownerNetId", _ownerNetId, timestep);
		}
		private int _playerID;
		public event FieldEvent<int> playerIDChanged;
		public Interpolated<int> playerIDInterpolation = new Interpolated<int>() { LerpT = 0f, Enabled = false };
		public int playerID
		{
			get { return _playerID; }
			set
			{
				// Don't do anything if the value is the same
				if (_playerID == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x2;
				_playerID = value;
				hasDirtyFields = true;
			}
		}

		public void SetplayerIDDirty()
		{
			_dirtyFields[0] |= 0x2;
			hasDirtyFields = true;
		}

		private void RunChange_playerID(ulong timestep)
		{
			if (playerIDChanged != null) playerIDChanged(_playerID, timestep);
			if (fieldAltered != null) fieldAltered("playerID", _playerID, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			ownerNetIdInterpolation.current = ownerNetIdInterpolation.target;
			playerIDInterpolation.current = playerIDInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _ownerNetId);
			UnityObjectMapper.Instance.MapBytes(data, _playerID);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_ownerNetId = UnityObjectMapper.Instance.Map<uint>(payload);
			ownerNetIdInterpolation.current = _ownerNetId;
			ownerNetIdInterpolation.target = _ownerNetId;
			RunChange_ownerNetId(timestep);
			_playerID = UnityObjectMapper.Instance.Map<int>(payload);
			playerIDInterpolation.current = _playerID;
			playerIDInterpolation.target = _playerID;
			RunChange_playerID(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _ownerNetId);
			if ((0x2 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _playerID);

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
			if ((0x2 & readDirtyFlags[0]) != 0)
			{
				if (playerIDInterpolation.Enabled)
				{
					playerIDInterpolation.target = UnityObjectMapper.Instance.Map<int>(data);
					playerIDInterpolation.Timestep = timestep;
				}
				else
				{
					_playerID = UnityObjectMapper.Instance.Map<int>(data);
					RunChange_playerID(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (ownerNetIdInterpolation.Enabled && !ownerNetIdInterpolation.current.UnityNear(ownerNetIdInterpolation.target, 0.0015f))
			{
				_ownerNetId = (uint)ownerNetIdInterpolation.Interpolate();
				//RunChange_ownerNetId(ownerNetIdInterpolation.Timestep);
			}
			if (playerIDInterpolation.Enabled && !playerIDInterpolation.current.UnityNear(playerIDInterpolation.target, 0.0015f))
			{
				_playerID = (int)playerIDInterpolation.Interpolate();
				//RunChange_playerID(playerIDInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public PlayerNetworkObject() : base() { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public PlayerNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
