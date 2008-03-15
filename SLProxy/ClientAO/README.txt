ClientAO by Issarlk Chatnoir



What is it?
***********

ClientAO is an implementation of an animation overrider running as a plugin for SLProxy.
Since it runs on the local computer and not on SL's server, this AO has no impact on sim
performance and lag. Also, it reacts to animation changes instantly instead of having to
poll the avatar state every so often, like inworld AO.

Instead of using an object like a regular AO, you point it to a folder in your inventory 
containing the animations and a configuration notecard.

The AO understands "wetikon" type notecards (see sample)


How to use
**********
- Run SLProxy and start your SecondLife client so that it connects through the Proxy 
(ex: SecondLife.exe ...(usual options here)... -loginuri http://localhost:8080/ )

- Login like usual

- once logged in, load the ClientAO plugin: /load ClientAO.dll

- Point it to a folder where you have previously put animations and a configuration 
  notecard, ex: /ao Objects/MyAO/*Default Anims

The AO will load the notecard.

- start the AO: /ao on

The AO should now replace your animations.
ClientAO by Issarlk Chatnoir



What is it?
***********

ClientAO is an implementation of an animation overrider running as a plugin for SLProxy.
Since it runs on the local computer and not on SL's server, this AO has no impact on sim
performance and lag. Also, it reacts to animation changes instantly instead of having to
poll the avatar state every so often, like inworld AO.

Instead of using an object like a regular AO, you point it to a folder in your inventory 
containing the animations and a configuration notecard.

The AO understands "wetikon" type notecards (see sample)


How to use
**********
- Run SLProxy and start your SecondLife client so that it connects through the Proxy 
(ex: SecondLife.exe ...(usual options here)... -loginuri http://localhost:8080/ )

- Login like usual

- once logged in, load the ClientAO plugin: /load ClientAO.dll

- Point it to a folder where you have previously put animations and a configuration 
  notecard, ex: /ao Objects/MyAO/*Default Anims

The AO will load the notecard.

- start the AO: /ao on

The AO should now replace your animations.
