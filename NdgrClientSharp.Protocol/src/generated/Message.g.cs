// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: dwango/nicolive/chat/data/message.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Dwango.Nicolive.Chat.Data {

  /// <summary>Holder for reflection information generated from dwango/nicolive/chat/data/message.proto</summary>
  public static partial class MessageReflection {

    #region Descriptor
    /// <summary>File descriptor for dwango/nicolive/chat/data/message.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static MessageReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cidkd2FuZ28vbmljb2xpdmUvY2hhdC9kYXRhL21lc3NhZ2UucHJvdG8SGWR3",
            "YW5nby5uaWNvbGl2ZS5jaGF0LmRhdGEaJWR3YW5nby9uaWNvbGl2ZS9jaGF0",
            "L2RhdGEvYXRvbXMucHJvdG8aL2R3YW5nby9uaWNvbGl2ZS9jaGF0L2RhdGEv",
            "YXRvbXMvbW9kZXJhdG9yLnByb3RvIt4ECg9OaWNvbGl2ZU1lc3NhZ2USLwoE",
            "Y2hhdBgBIAEoCzIfLmR3YW5nby5uaWNvbGl2ZS5jaGF0LmRhdGEuQ2hhdEgA",
            "EkwKE3NpbXBsZV9ub3RpZmljYXRpb24YByABKAsyLS5kd2FuZ28ubmljb2xp",
            "dmUuY2hhdC5kYXRhLlNpbXBsZU5vdGlmaWNhdGlvbkgAEi8KBGdpZnQYCCAB",
            "KAsyHy5kd2FuZ28ubmljb2xpdmUuY2hhdC5kYXRhLkdpZnRIABIzCgZuaWNv",
            "YWQYCSABKAsyIS5kd2FuZ28ubmljb2xpdmUuY2hhdC5kYXRhLk5pY29hZEgA",
            "EjwKC2dhbWVfdXBkYXRlGA0gASgLMiUuZHdhbmdvLm5pY29saXZlLmNoYXQu",
            "ZGF0YS5HYW1lVXBkYXRlSAASPAoLdGFnX3VwZGF0ZWQYESABKAsyJS5kd2Fu",
            "Z28ubmljb2xpdmUuY2hhdC5kYXRhLlRhZ1VwZGF0ZWRIABJOChFtb2RlcmF0",
            "b3JfdXBkYXRlZBgSIAEoCzIxLmR3YW5nby5uaWNvbGl2ZS5jaGF0LmRhdGEu",
            "YXRvbXMuTW9kZXJhdG9yVXBkYXRlZEgAEkQKDHNzbmdfdXBkYXRlZBgTIAEo",
            "CzIsLmR3YW5nby5uaWNvbGl2ZS5jaGF0LmRhdGEuYXRvbXMuU1NOR1VwZGF0",
            "ZWRIABI6Cg9vdmVyZmxvd2VkX2NoYXQYFCABKAsyHy5kd2FuZ28ubmljb2xp",
            "dmUuY2hhdC5kYXRhLkNoYXRIAEIGCgRkYXRhSgQIAhAHSgQIChANSgQIDhAR",
            "YgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Dwango.Nicolive.Chat.Data.AtomsReflection.Descriptor, global::Dwango.Nicolive.Chat.Data.Atoms.ModeratorReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Dwango.Nicolive.Chat.Data.NicoliveMessage), global::Dwango.Nicolive.Chat.Data.NicoliveMessage.Parser, new[]{ "Chat", "SimpleNotification", "Gift", "Nicoad", "GameUpdate", "TagUpdated", "ModeratorUpdated", "SsngUpdated", "OverflowedChat" }, new[]{ "Data" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class NicoliveMessage : pb::IMessage<NicoliveMessage>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<NicoliveMessage> _parser = new pb::MessageParser<NicoliveMessage>(() => new NicoliveMessage());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<NicoliveMessage> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Dwango.Nicolive.Chat.Data.MessageReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NicoliveMessage() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NicoliveMessage(NicoliveMessage other) : this() {
      switch (other.DataCase) {
        case DataOneofCase.Chat:
          Chat = other.Chat.Clone();
          break;
        case DataOneofCase.SimpleNotification:
          SimpleNotification = other.SimpleNotification.Clone();
          break;
        case DataOneofCase.Gift:
          Gift = other.Gift.Clone();
          break;
        case DataOneofCase.Nicoad:
          Nicoad = other.Nicoad.Clone();
          break;
        case DataOneofCase.GameUpdate:
          GameUpdate = other.GameUpdate.Clone();
          break;
        case DataOneofCase.TagUpdated:
          TagUpdated = other.TagUpdated.Clone();
          break;
        case DataOneofCase.ModeratorUpdated:
          ModeratorUpdated = other.ModeratorUpdated.Clone();
          break;
        case DataOneofCase.SsngUpdated:
          SsngUpdated = other.SsngUpdated.Clone();
          break;
        case DataOneofCase.OverflowedChat:
          OverflowedChat = other.OverflowedChat.Clone();
          break;
      }

      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NicoliveMessage Clone() {
      return new NicoliveMessage(this);
    }

    /// <summary>Field number for the "chat" field.</summary>
    public const int ChatFieldNumber = 1;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Dwango.Nicolive.Chat.Data.Chat Chat {
      get { return dataCase_ == DataOneofCase.Chat ? (global::Dwango.Nicolive.Chat.Data.Chat) data_ : null; }
      set {
        data_ = value;
        dataCase_ = value == null ? DataOneofCase.None : DataOneofCase.Chat;
      }
    }

    /// <summary>Field number for the "simple_notification" field.</summary>
    public const int SimpleNotificationFieldNumber = 7;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Dwango.Nicolive.Chat.Data.SimpleNotification SimpleNotification {
      get { return dataCase_ == DataOneofCase.SimpleNotification ? (global::Dwango.Nicolive.Chat.Data.SimpleNotification) data_ : null; }
      set {
        data_ = value;
        dataCase_ = value == null ? DataOneofCase.None : DataOneofCase.SimpleNotification;
      }
    }

    /// <summary>Field number for the "gift" field.</summary>
    public const int GiftFieldNumber = 8;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Dwango.Nicolive.Chat.Data.Gift Gift {
      get { return dataCase_ == DataOneofCase.Gift ? (global::Dwango.Nicolive.Chat.Data.Gift) data_ : null; }
      set {
        data_ = value;
        dataCase_ = value == null ? DataOneofCase.None : DataOneofCase.Gift;
      }
    }

    /// <summary>Field number for the "nicoad" field.</summary>
    public const int NicoadFieldNumber = 9;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Dwango.Nicolive.Chat.Data.Nicoad Nicoad {
      get { return dataCase_ == DataOneofCase.Nicoad ? (global::Dwango.Nicolive.Chat.Data.Nicoad) data_ : null; }
      set {
        data_ = value;
        dataCase_ = value == null ? DataOneofCase.None : DataOneofCase.Nicoad;
      }
    }

    /// <summary>Field number for the "game_update" field.</summary>
    public const int GameUpdateFieldNumber = 13;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Dwango.Nicolive.Chat.Data.GameUpdate GameUpdate {
      get { return dataCase_ == DataOneofCase.GameUpdate ? (global::Dwango.Nicolive.Chat.Data.GameUpdate) data_ : null; }
      set {
        data_ = value;
        dataCase_ = value == null ? DataOneofCase.None : DataOneofCase.GameUpdate;
      }
    }

    /// <summary>Field number for the "tag_updated" field.</summary>
    public const int TagUpdatedFieldNumber = 17;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Dwango.Nicolive.Chat.Data.TagUpdated TagUpdated {
      get { return dataCase_ == DataOneofCase.TagUpdated ? (global::Dwango.Nicolive.Chat.Data.TagUpdated) data_ : null; }
      set {
        data_ = value;
        dataCase_ = value == null ? DataOneofCase.None : DataOneofCase.TagUpdated;
      }
    }

    /// <summary>Field number for the "moderator_updated" field.</summary>
    public const int ModeratorUpdatedFieldNumber = 18;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Dwango.Nicolive.Chat.Data.Atoms.ModeratorUpdated ModeratorUpdated {
      get { return dataCase_ == DataOneofCase.ModeratorUpdated ? (global::Dwango.Nicolive.Chat.Data.Atoms.ModeratorUpdated) data_ : null; }
      set {
        data_ = value;
        dataCase_ = value == null ? DataOneofCase.None : DataOneofCase.ModeratorUpdated;
      }
    }

    /// <summary>Field number for the "ssng_updated" field.</summary>
    public const int SsngUpdatedFieldNumber = 19;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Dwango.Nicolive.Chat.Data.Atoms.SSNGUpdated SsngUpdated {
      get { return dataCase_ == DataOneofCase.SsngUpdated ? (global::Dwango.Nicolive.Chat.Data.Atoms.SSNGUpdated) data_ : null; }
      set {
        data_ = value;
        dataCase_ = value == null ? DataOneofCase.None : DataOneofCase.SsngUpdated;
      }
    }

    /// <summary>Field number for the "overflowed_chat" field.</summary>
    public const int OverflowedChatFieldNumber = 20;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Dwango.Nicolive.Chat.Data.Chat OverflowedChat {
      get { return dataCase_ == DataOneofCase.OverflowedChat ? (global::Dwango.Nicolive.Chat.Data.Chat) data_ : null; }
      set {
        data_ = value;
        dataCase_ = value == null ? DataOneofCase.None : DataOneofCase.OverflowedChat;
      }
    }

    private object data_;
    /// <summary>Enum of possible cases for the "data" oneof.</summary>
    public enum DataOneofCase {
      None = 0,
      Chat = 1,
      SimpleNotification = 7,
      Gift = 8,
      Nicoad = 9,
      GameUpdate = 13,
      TagUpdated = 17,
      ModeratorUpdated = 18,
      SsngUpdated = 19,
      OverflowedChat = 20,
    }
    private DataOneofCase dataCase_ = DataOneofCase.None;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public DataOneofCase DataCase {
      get { return dataCase_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearData() {
      dataCase_ = DataOneofCase.None;
      data_ = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as NicoliveMessage);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(NicoliveMessage other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Chat, other.Chat)) return false;
      if (!object.Equals(SimpleNotification, other.SimpleNotification)) return false;
      if (!object.Equals(Gift, other.Gift)) return false;
      if (!object.Equals(Nicoad, other.Nicoad)) return false;
      if (!object.Equals(GameUpdate, other.GameUpdate)) return false;
      if (!object.Equals(TagUpdated, other.TagUpdated)) return false;
      if (!object.Equals(ModeratorUpdated, other.ModeratorUpdated)) return false;
      if (!object.Equals(SsngUpdated, other.SsngUpdated)) return false;
      if (!object.Equals(OverflowedChat, other.OverflowedChat)) return false;
      if (DataCase != other.DataCase) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (dataCase_ == DataOneofCase.Chat) hash ^= Chat.GetHashCode();
      if (dataCase_ == DataOneofCase.SimpleNotification) hash ^= SimpleNotification.GetHashCode();
      if (dataCase_ == DataOneofCase.Gift) hash ^= Gift.GetHashCode();
      if (dataCase_ == DataOneofCase.Nicoad) hash ^= Nicoad.GetHashCode();
      if (dataCase_ == DataOneofCase.GameUpdate) hash ^= GameUpdate.GetHashCode();
      if (dataCase_ == DataOneofCase.TagUpdated) hash ^= TagUpdated.GetHashCode();
      if (dataCase_ == DataOneofCase.ModeratorUpdated) hash ^= ModeratorUpdated.GetHashCode();
      if (dataCase_ == DataOneofCase.SsngUpdated) hash ^= SsngUpdated.GetHashCode();
      if (dataCase_ == DataOneofCase.OverflowedChat) hash ^= OverflowedChat.GetHashCode();
      hash ^= (int) dataCase_;
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (dataCase_ == DataOneofCase.Chat) {
        output.WriteRawTag(10);
        output.WriteMessage(Chat);
      }
      if (dataCase_ == DataOneofCase.SimpleNotification) {
        output.WriteRawTag(58);
        output.WriteMessage(SimpleNotification);
      }
      if (dataCase_ == DataOneofCase.Gift) {
        output.WriteRawTag(66);
        output.WriteMessage(Gift);
      }
      if (dataCase_ == DataOneofCase.Nicoad) {
        output.WriteRawTag(74);
        output.WriteMessage(Nicoad);
      }
      if (dataCase_ == DataOneofCase.GameUpdate) {
        output.WriteRawTag(106);
        output.WriteMessage(GameUpdate);
      }
      if (dataCase_ == DataOneofCase.TagUpdated) {
        output.WriteRawTag(138, 1);
        output.WriteMessage(TagUpdated);
      }
      if (dataCase_ == DataOneofCase.ModeratorUpdated) {
        output.WriteRawTag(146, 1);
        output.WriteMessage(ModeratorUpdated);
      }
      if (dataCase_ == DataOneofCase.SsngUpdated) {
        output.WriteRawTag(154, 1);
        output.WriteMessage(SsngUpdated);
      }
      if (dataCase_ == DataOneofCase.OverflowedChat) {
        output.WriteRawTag(162, 1);
        output.WriteMessage(OverflowedChat);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (dataCase_ == DataOneofCase.Chat) {
        output.WriteRawTag(10);
        output.WriteMessage(Chat);
      }
      if (dataCase_ == DataOneofCase.SimpleNotification) {
        output.WriteRawTag(58);
        output.WriteMessage(SimpleNotification);
      }
      if (dataCase_ == DataOneofCase.Gift) {
        output.WriteRawTag(66);
        output.WriteMessage(Gift);
      }
      if (dataCase_ == DataOneofCase.Nicoad) {
        output.WriteRawTag(74);
        output.WriteMessage(Nicoad);
      }
      if (dataCase_ == DataOneofCase.GameUpdate) {
        output.WriteRawTag(106);
        output.WriteMessage(GameUpdate);
      }
      if (dataCase_ == DataOneofCase.TagUpdated) {
        output.WriteRawTag(138, 1);
        output.WriteMessage(TagUpdated);
      }
      if (dataCase_ == DataOneofCase.ModeratorUpdated) {
        output.WriteRawTag(146, 1);
        output.WriteMessage(ModeratorUpdated);
      }
      if (dataCase_ == DataOneofCase.SsngUpdated) {
        output.WriteRawTag(154, 1);
        output.WriteMessage(SsngUpdated);
      }
      if (dataCase_ == DataOneofCase.OverflowedChat) {
        output.WriteRawTag(162, 1);
        output.WriteMessage(OverflowedChat);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (dataCase_ == DataOneofCase.Chat) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Chat);
      }
      if (dataCase_ == DataOneofCase.SimpleNotification) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(SimpleNotification);
      }
      if (dataCase_ == DataOneofCase.Gift) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Gift);
      }
      if (dataCase_ == DataOneofCase.Nicoad) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Nicoad);
      }
      if (dataCase_ == DataOneofCase.GameUpdate) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(GameUpdate);
      }
      if (dataCase_ == DataOneofCase.TagUpdated) {
        size += 2 + pb::CodedOutputStream.ComputeMessageSize(TagUpdated);
      }
      if (dataCase_ == DataOneofCase.ModeratorUpdated) {
        size += 2 + pb::CodedOutputStream.ComputeMessageSize(ModeratorUpdated);
      }
      if (dataCase_ == DataOneofCase.SsngUpdated) {
        size += 2 + pb::CodedOutputStream.ComputeMessageSize(SsngUpdated);
      }
      if (dataCase_ == DataOneofCase.OverflowedChat) {
        size += 2 + pb::CodedOutputStream.ComputeMessageSize(OverflowedChat);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(NicoliveMessage other) {
      if (other == null) {
        return;
      }
      switch (other.DataCase) {
        case DataOneofCase.Chat:
          if (Chat == null) {
            Chat = new global::Dwango.Nicolive.Chat.Data.Chat();
          }
          Chat.MergeFrom(other.Chat);
          break;
        case DataOneofCase.SimpleNotification:
          if (SimpleNotification == null) {
            SimpleNotification = new global::Dwango.Nicolive.Chat.Data.SimpleNotification();
          }
          SimpleNotification.MergeFrom(other.SimpleNotification);
          break;
        case DataOneofCase.Gift:
          if (Gift == null) {
            Gift = new global::Dwango.Nicolive.Chat.Data.Gift();
          }
          Gift.MergeFrom(other.Gift);
          break;
        case DataOneofCase.Nicoad:
          if (Nicoad == null) {
            Nicoad = new global::Dwango.Nicolive.Chat.Data.Nicoad();
          }
          Nicoad.MergeFrom(other.Nicoad);
          break;
        case DataOneofCase.GameUpdate:
          if (GameUpdate == null) {
            GameUpdate = new global::Dwango.Nicolive.Chat.Data.GameUpdate();
          }
          GameUpdate.MergeFrom(other.GameUpdate);
          break;
        case DataOneofCase.TagUpdated:
          if (TagUpdated == null) {
            TagUpdated = new global::Dwango.Nicolive.Chat.Data.TagUpdated();
          }
          TagUpdated.MergeFrom(other.TagUpdated);
          break;
        case DataOneofCase.ModeratorUpdated:
          if (ModeratorUpdated == null) {
            ModeratorUpdated = new global::Dwango.Nicolive.Chat.Data.Atoms.ModeratorUpdated();
          }
          ModeratorUpdated.MergeFrom(other.ModeratorUpdated);
          break;
        case DataOneofCase.SsngUpdated:
          if (SsngUpdated == null) {
            SsngUpdated = new global::Dwango.Nicolive.Chat.Data.Atoms.SSNGUpdated();
          }
          SsngUpdated.MergeFrom(other.SsngUpdated);
          break;
        case DataOneofCase.OverflowedChat:
          if (OverflowedChat == null) {
            OverflowedChat = new global::Dwango.Nicolive.Chat.Data.Chat();
          }
          OverflowedChat.MergeFrom(other.OverflowedChat);
          break;
      }

      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            global::Dwango.Nicolive.Chat.Data.Chat subBuilder = new global::Dwango.Nicolive.Chat.Data.Chat();
            if (dataCase_ == DataOneofCase.Chat) {
              subBuilder.MergeFrom(Chat);
            }
            input.ReadMessage(subBuilder);
            Chat = subBuilder;
            break;
          }
          case 58: {
            global::Dwango.Nicolive.Chat.Data.SimpleNotification subBuilder = new global::Dwango.Nicolive.Chat.Data.SimpleNotification();
            if (dataCase_ == DataOneofCase.SimpleNotification) {
              subBuilder.MergeFrom(SimpleNotification);
            }
            input.ReadMessage(subBuilder);
            SimpleNotification = subBuilder;
            break;
          }
          case 66: {
            global::Dwango.Nicolive.Chat.Data.Gift subBuilder = new global::Dwango.Nicolive.Chat.Data.Gift();
            if (dataCase_ == DataOneofCase.Gift) {
              subBuilder.MergeFrom(Gift);
            }
            input.ReadMessage(subBuilder);
            Gift = subBuilder;
            break;
          }
          case 74: {
            global::Dwango.Nicolive.Chat.Data.Nicoad subBuilder = new global::Dwango.Nicolive.Chat.Data.Nicoad();
            if (dataCase_ == DataOneofCase.Nicoad) {
              subBuilder.MergeFrom(Nicoad);
            }
            input.ReadMessage(subBuilder);
            Nicoad = subBuilder;
            break;
          }
          case 106: {
            global::Dwango.Nicolive.Chat.Data.GameUpdate subBuilder = new global::Dwango.Nicolive.Chat.Data.GameUpdate();
            if (dataCase_ == DataOneofCase.GameUpdate) {
              subBuilder.MergeFrom(GameUpdate);
            }
            input.ReadMessage(subBuilder);
            GameUpdate = subBuilder;
            break;
          }
          case 138: {
            global::Dwango.Nicolive.Chat.Data.TagUpdated subBuilder = new global::Dwango.Nicolive.Chat.Data.TagUpdated();
            if (dataCase_ == DataOneofCase.TagUpdated) {
              subBuilder.MergeFrom(TagUpdated);
            }
            input.ReadMessage(subBuilder);
            TagUpdated = subBuilder;
            break;
          }
          case 146: {
            global::Dwango.Nicolive.Chat.Data.Atoms.ModeratorUpdated subBuilder = new global::Dwango.Nicolive.Chat.Data.Atoms.ModeratorUpdated();
            if (dataCase_ == DataOneofCase.ModeratorUpdated) {
              subBuilder.MergeFrom(ModeratorUpdated);
            }
            input.ReadMessage(subBuilder);
            ModeratorUpdated = subBuilder;
            break;
          }
          case 154: {
            global::Dwango.Nicolive.Chat.Data.Atoms.SSNGUpdated subBuilder = new global::Dwango.Nicolive.Chat.Data.Atoms.SSNGUpdated();
            if (dataCase_ == DataOneofCase.SsngUpdated) {
              subBuilder.MergeFrom(SsngUpdated);
            }
            input.ReadMessage(subBuilder);
            SsngUpdated = subBuilder;
            break;
          }
          case 162: {
            global::Dwango.Nicolive.Chat.Data.Chat subBuilder = new global::Dwango.Nicolive.Chat.Data.Chat();
            if (dataCase_ == DataOneofCase.OverflowedChat) {
              subBuilder.MergeFrom(OverflowedChat);
            }
            input.ReadMessage(subBuilder);
            OverflowedChat = subBuilder;
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            global::Dwango.Nicolive.Chat.Data.Chat subBuilder = new global::Dwango.Nicolive.Chat.Data.Chat();
            if (dataCase_ == DataOneofCase.Chat) {
              subBuilder.MergeFrom(Chat);
            }
            input.ReadMessage(subBuilder);
            Chat = subBuilder;
            break;
          }
          case 58: {
            global::Dwango.Nicolive.Chat.Data.SimpleNotification subBuilder = new global::Dwango.Nicolive.Chat.Data.SimpleNotification();
            if (dataCase_ == DataOneofCase.SimpleNotification) {
              subBuilder.MergeFrom(SimpleNotification);
            }
            input.ReadMessage(subBuilder);
            SimpleNotification = subBuilder;
            break;
          }
          case 66: {
            global::Dwango.Nicolive.Chat.Data.Gift subBuilder = new global::Dwango.Nicolive.Chat.Data.Gift();
            if (dataCase_ == DataOneofCase.Gift) {
              subBuilder.MergeFrom(Gift);
            }
            input.ReadMessage(subBuilder);
            Gift = subBuilder;
            break;
          }
          case 74: {
            global::Dwango.Nicolive.Chat.Data.Nicoad subBuilder = new global::Dwango.Nicolive.Chat.Data.Nicoad();
            if (dataCase_ == DataOneofCase.Nicoad) {
              subBuilder.MergeFrom(Nicoad);
            }
            input.ReadMessage(subBuilder);
            Nicoad = subBuilder;
            break;
          }
          case 106: {
            global::Dwango.Nicolive.Chat.Data.GameUpdate subBuilder = new global::Dwango.Nicolive.Chat.Data.GameUpdate();
            if (dataCase_ == DataOneofCase.GameUpdate) {
              subBuilder.MergeFrom(GameUpdate);
            }
            input.ReadMessage(subBuilder);
            GameUpdate = subBuilder;
            break;
          }
          case 138: {
            global::Dwango.Nicolive.Chat.Data.TagUpdated subBuilder = new global::Dwango.Nicolive.Chat.Data.TagUpdated();
            if (dataCase_ == DataOneofCase.TagUpdated) {
              subBuilder.MergeFrom(TagUpdated);
            }
            input.ReadMessage(subBuilder);
            TagUpdated = subBuilder;
            break;
          }
          case 146: {
            global::Dwango.Nicolive.Chat.Data.Atoms.ModeratorUpdated subBuilder = new global::Dwango.Nicolive.Chat.Data.Atoms.ModeratorUpdated();
            if (dataCase_ == DataOneofCase.ModeratorUpdated) {
              subBuilder.MergeFrom(ModeratorUpdated);
            }
            input.ReadMessage(subBuilder);
            ModeratorUpdated = subBuilder;
            break;
          }
          case 154: {
            global::Dwango.Nicolive.Chat.Data.Atoms.SSNGUpdated subBuilder = new global::Dwango.Nicolive.Chat.Data.Atoms.SSNGUpdated();
            if (dataCase_ == DataOneofCase.SsngUpdated) {
              subBuilder.MergeFrom(SsngUpdated);
            }
            input.ReadMessage(subBuilder);
            SsngUpdated = subBuilder;
            break;
          }
          case 162: {
            global::Dwango.Nicolive.Chat.Data.Chat subBuilder = new global::Dwango.Nicolive.Chat.Data.Chat();
            if (dataCase_ == DataOneofCase.OverflowedChat) {
              subBuilder.MergeFrom(OverflowedChat);
            }
            input.ReadMessage(subBuilder);
            OverflowedChat = subBuilder;
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
