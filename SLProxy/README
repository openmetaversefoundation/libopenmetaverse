SLProxy is a library that works in conjunction with libsecondlife to
allow applications to wedge themselves between the official Second
Life client and servers.  SLProxy applications can inspect and modify
any packet as it passes between the client and the servers; remove
packets from the stream; and inject new packets into the stream.
SLProxy automatically takes care of tracking circuits and modifying
sequence numbers and acknowledgements to account for changes to the
packet stream.

The continued existence of this software of course rests on the good
will of Linden Lab toward the Second Life reverse engineering effort.
Please use common sense when designing applications and report any
security holes you may find to security@lindenlab.com.

To use an SLProxy application, you must first start the proxy, then
start Second Life with the switch `-loginuri http://localhost:8080/'.
In Windows, add this switch (without the quotes) to your Second Life
shortcut.  In MacOS X, this can be accomplished by sending the
following commands to the Terminal (assuming Second Life is installed
in /Applications):

	cd "/Applications/Second Life.app"
	"Contents/MacOS/Second Life" -loginuri http://localhost:8080/

Note that for security reasons, by default, SLProxy applications must
be running on the same computer as Second Life.  If you need to run a
proxy on a different machine or port, start the proxy with the
--proxy-help switch and see the options available.

SLProxy can only handle one client using a proxy at a time.

BUILDING
========

To build SLProxy, you must check out the entire libsecondlife trunk
with subversion:

  svn co svn://svn.gna.org/svn/libsecondlife/trunk libsecondlife

The libsecondlife-cs project must be built first; see
libsecondlife-cs/README for instructions.  Building SLProxy should be
straightforward with Microsoft Visual Studio.  If you're using Mono,
you can build the solution with the included build script:

  perl build

The SLProxy library and its example applications will be built in
bin/Debug.  In order to run the example applications, you must first
add the libsecondlife-cs build directory to your MONO_PATH environment
variable.  For example, if your libsecondlife-cs directory is
~/libsecondlife/libsecondlife-cs and your shell is bash, you can type:

  export MONO_PATH=$MONO_PATH:~/libsecondlife/libsecondlife-cs/bin/Debug/

INCLUDED APPLICATIONS
=====================

Included with SLProxy are a few example application, which are covered
in this section.

1. Analyst
----------

Analyst makes SLProxy's packet inspection and modification
functionality interactive.  When connected to Second Life through
Analyst, you use the following commands by saying them using in-world
chat:

/log <packet name>

  Packets of type <packet name> will be dumped to the console.  For
  example, say `/log ChatFromSimulator' to get a packet dump of all
  incoming chat.

/-log <packet name>

  Packets of type <packet name> will no longer be dumped to the
  console.

/log *

  All packets will be dumped to the console.

/-log *

  No packets will be dumped to the console.

/grep [regex]

  Only log packets that have a field for which regex matches
  <packet name>.<block name>.<field name> = <value>.  To stop
  filtering, type /grep without an argument.  Matches are case
  insensitive.  In the case of a variable field, Analyst will try to
  convert it into a string; if that doesn't match, it will try
  converting it into a hexidecimal numeral preceeded by 0x.

/set <packet name> <block name> <field name> <value>

  All forthcoming packets of type <packet name> will have the field
  identified by <block name> and <field name> set to <value>.  For
  example, if you say `/set ChatFromViewer ChatData Type 0',
  everything you say thereafter will be whispered.  Values for
  variable fields will be interpreted as strings unless they begin
  with a 0x, in which case they will be treated as hexidecimal
  numerals representing the contents of the field.

/-set <packet name> <block name> <field name>

  Packets of type <packet name> will no longer have the field
  identified by <block name> and <field name> modified.

/-set *

  No fields will be modified.

/inject <packet file> [value]

  Inject the packet described by <packet file>.packet in the working
  directory.  The [value] is optional and may be required by some
  packet files.  `/in' is an alias for `/inject'.  The syntax of a
  packet file is described in section 2.1.  SLProxy comes with two
  example packet files: god.packet allows you to enable hacked god
  mode by typing `/inject god', and whisper.packet allows you to
  whisper by typing `/inject whisper <message>'.

These commands will not be forwarded to the server, so other people
won't hear you say them.

Analyst accepts a --log-all command line switch, which causes the
proxy to start out logging all packets as if you had typed `/log *'.
This can be useful if you want to capture a complete dump of your
session, including login.

Analyst also accepts a --log-login command line switch, which causes
the XML-RPC login request and response to dumped to the console.

2.1 Packet files
- - - - - - - -

A packet file describes a packet that can be injected with the /inject
command.  Please refer to god.packet and whisper.packet (in the
bin/Debug/ directory) as examples.

The first line of a packet file must contain the word `in' or `out',
specifying whether the packet is incoming or outgoing, respectively,
followed by the name of the packet.

The remainder of the file specifies the packet's blocks and fields.  A
block is described by placing its name in square brackets
(e.g. `[GrantData]').  Following the the line specifying the block's
name, the block's fields and values are specified, separated by equal
signs (e.g. `GodLevel = 255'), one per line.

The value of a field can be a literal value (e.g. `255'), or one of
the following special values:

$Value

  the [value] specified by the user

$UUID

  a random UUID

$AgentID

  the user's AgentID

$SessionID

  the user's SessionID

2. ChatConsole
--------------

ChatConsole is a trivial SLProxy application intended as an example of
how SLProxy applications can be written.  When connected to Second
Life through ChatConsole, all in-world chat will be echoed to the
console, and anything typed in the console will be echoed to the game as
in-world chat.

PUBLIC INTERFACE
================

This section describes the interface that SLProxy applications will
use to interact with the packet stream.  Please see ChatConsole.cs for
a simple example of how this interface can be used.

SLProxy extends the functionality of libsecondlife, so we assume here
that the reader is already familiar with libsecondlife's Packet and
PacketBuilder classes.

1. ProxyConfig class
--------------------

An instance of ProxyConfig represents the configuration of a Proxy
object, and must be provided when constructing a Proxy.  ProxyConfig
has two constructors:

	ProxyConfig(string userAgent, string author)
	ProxyConfig(string userAgent, string author, string[] args)

Both constructors require a user agent name and the author's email
address.  These are sent to Second Life's login server to identify the
client, and to allow Linden Lab to get in touch with authors whose
applications may inadvertantly be causing problems.  The second
constructor is preferred and takes an array of command-line arguments
that allow the user to override certain network settings.  For a list
of command line arguments, start your appliation with the --proxy-help
switch.

2. Proxy class
--------------

The Proxy class represents an instance of an SLProxy and provides the
methods necessary to modify the packet stream.  Proxy's sole
constructor takes an instance of ProxyConfig.

2.1 Login delegates
- - - - - - - - - -

You may specify that SLProxy should call a delegate method in your
application when the user requests login or the server responds.

	delegate void XmlRpcRequestDelegate(XmlRpcRequest request)
	delegate void XmlRpcResponseDelegate(XmlRpcResponse response)
	void SetLoginRequestDelegate(XmlRpcRequestDelegate loginRequestDelegate)
	void SetLoginResponseDelegate(XmlRpcResponseDelegate loginResponseDelegate)

A login response delegate, in particular, is useful for retrieving the
agent_id and session_id, which are required when injecting certain
types of packets.  See ChatConsole.cs for an example of how these can
be retrieved.

Note that all delegates must terminate (not go into an infinite loop),
and must be thread-safe.

2.2 Packet delegates
- - - - - - - - - -

Packet delegates allow you to inspect and modify packets as they pass
between the client and the server:

	delegate Packet PacketDelegate(Packet packet, IPEndPoint endPoint)
	void AddDelegate(string packetName, Direction direction, PacketDelegate packetDelegate)
	void RemoveDelegate(string packetName, Direction direction)

AddDelegate adds a callback delegate for packets named packetName
going direction.  Directions are either Direction.Incoming, meaning
packets heading from the server to the client, or Direction.Outgoing,
meaning packets heading from the client to the server.  Only one
delegate can apply to a packet at a time; if you add a new delegate
with the same packetName and direction, the old one will be removed.

RemoveDelegate simply removes the delegate for the specified type of
packet.

PacketDelegate methods are passed a copy of the packet (in the form of
a libsecondlife Packet object) and the IPEndPoint of the server that
sent (or will receive) the packet.  PacketDelegate methods may do one
of three things:

1. Return the same packet, in which case it will be passed on.
2. Return a new packet (built with libsecondlife), in which case the
   new packet will substitute for the original.  SLProxy will
   automatically copy the sequence number and appended ACKs from the
   old packet to the new one.
3. Return null, in which case the packet will not be passed on.

SLProxy automatically takes care of ensuring that sequence numbers and
acknowledgements are adjusted to account for changes made by the
application.  When replacing a reliable packet with an unreliable
packet or removing a reliable packet, a fake acknowledgement is
injected.  When replacing an unreliable packet with a reliable packet,
SLProxy ensures delivery and intercepts its acknowledgement.  Note
that if a reliable packet is passed on but then lost on the network,
Second Life will resend it and the delegate will be called again.  You
can tell if a packet is being resent by checking if (packet.Data[0] &
Helpers.MSG_RESENT) is nonzero, although be advised that it's possible
that the original packet never made it to the proxy and the packet
will be marked RESENT the first time the proxy ever sees it.

Note that all delegates must terminate (not go into an infinite loop),
and must be thread-safe.

2.3 Packet injection
- - - - - - - - - -

New packets may be injected into the stream at any point, either
during a delegate callback or by another thread in your application.
Packets are injected with the InjectPacket method:

	void InjectPacket(Packet packet, Direction direction)

This will inject a packet heading to either the client or to the
active server, when direction is Direction.Incoming or
Direction.Outgoing, respectively.  The packet's sequence number will
be set automatically, and if the packet is reliable, SLProxy will
ensure its delivery and intercept its acknowledgement.

Injecting a packet immediately upon (or prior to) connection is not
recommended, since the client and the server won't have initialized
their session yet.

2.4 Starting and stopping the proxy
- - - - - - - - - - - - - - - - - -

Once you've constructed a Proxy and added your delegates, you must
start it with the Start method:

	void Start()

Once started, the proxy will begin listening for connections.  The
Start method spawns new threads for the proxy and returns immediately.

When your application is ready to shut down, you must call the Stop
method:

	void Stop()

Note that this may not actually force the proxy to stop accepting
connections; it merely guarantees that all foreground threads are
stopped, allowing the application to exit cleanly.

3. PacketUtility class
----------------------

The PacketUtility class provides a handful of static methods which may
be useful when inspecting and modifying packets.

3.1 Hashtable Unbuild(Packet packet)
- - - - - - - - - - - - - - - - - -

The Unbuild method takes a Packet object and returns a table of
blocks, structured in a format suitable for passing to PacketUtility's
GetField and SetField methods or PacketBuilder's BuildPacket method.

For example, this should make an approximate copy of a packet:

Hashtable packetBlocks = PacketUtility.Unbuild(packet);
Packet packetCopy = PacketBuilder.BuildPacket(packet.Layout.Name, protocolManager, packetBlocks, packet.Data[0]);

3.2 object GetField(Hashtable blocks, string block, string field)
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

The GetField method takes a table of blocks (produced by
PacketUtility.Unbuild) and extracts the value of a particular field.
If the field is part of a variable block, an arbitrary instance of the
field will be returned.  If the field does not exist, null will be
returned.

3.3 void SetField(Hashtable blocks, string block, string field, object value)
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

The SetField method takes a table of blocks (produced by
PacketUtility.Unbuild) and sets the value of a particular field.  If
the field is part of a variable block, all instances of the field will
be set.  If the field does not exist, SetField will have no effect.

This can be used by a packet delegate method in conjunction with
PacketUtility.Unbuild and PacketBuilder.BuildPacket to substitute a
new packet that is a copy of the original packet with certain fields
modified.
