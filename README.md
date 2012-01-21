MC Mod Manager
==============

What is it?
-----------

A tool to easily manage and install Minecraft mods.

Why do you need it?
-------------------

Mods come in a few different flavours, and it gets hard to keep track. Some mods require editing Minecraft.jar, others depend on other mods to be installed first. It seems as though every mod is slightly different, and if you get it wrong, you get a black screen or a crash.

MC Mod Manager is designed to keep track of all these dependencies and ensure that your mod is installed correctly the first time with as little hassle as possible, even when a new version of Minecraft or the mod is released, and everything has to be re-installed.

What does it do?
----------------

MC Mod Manager does three distinct things:

 1. It keeps a record of all the mods you have installed
 2. It downloads new and updated mods for you
 3. It installs mods in the correct order based on their dependencies

All this is driven by metadata collected for the mod in the form of a Manifest. The Manifest describes all the versions of the mod, what mods each version depends on, and where to find updates.

What _doesn't_ it do?
---------------------

MC Mod Manager cannot make incompatible mods work together, nor can it update mods to work with newer versions of Minecraft. These are the responsibility of the mod creators.

Further, MC Mod Manager is not designed to work with Bukkit or other mods on the server side (though this may change in the future).

What is the development roadmap?
--------------------------------

The first thing is to get the client running. This is the program that will do the work of managing mods.

There will also be a need for a centralized repository of Manifests, so that the client is able to locate any dependency mods it requires. This may also double as hosting site where mod creators can explicitly host Manifest files.

In the future, it _may_ be possible to merge mods on a finer level to eliminate conflicts where the same class file is modified by two mods, but different methods in each. This would need extensive research into Java Bytecode, and is not a priority until at least version 2.0.