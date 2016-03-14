# FF6ExpED

creator: madsiur\\
version: 0.1

FF6ExpED is an FF3us (FF6) WPF editor that aims to expand some data of the game, manage these expansions and allow data editing.
Right now only dropped items by monsters can be edited. They are expanded to 4 per monster instead of 2 and the global drop chances can be edited.

In future version, I'll do similar implementations for rages, sketch and item stolen data.

Known bugs: 

* Total percentage can sometime be 101%. This is due to how the percentages are rounded.
* Backup feature does not work as intented. Need further investigation.
* Having a 256/256 chance drop with three other at 0/256 chance will most likely trigger a crash or unexpected result since the game will try to fit 256 on a byte type.
* NOT A BUG: UI styling is not completed.
