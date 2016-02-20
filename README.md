# NoMoreMissing
 
NoMoreMissing fixes missing scripts in unity project.

## Note

**MUST BACKUP** your project before using thie plugin that marks a behaviour **CLASS NAME** in **ALL** prefabs. 

## Examples

* Make a game object

    Hierarchy:

        example

* Make a behaviour

    Project:
        
        Assets/
            TestBehaviour.cs

* Add a behaviour to the game object 

    Inspector:

        Transform:
            ...

        TestBehaviour:
            Name: john
            Score: 75 

* Save a prefab for the game object

    Project:
        
        Assets/
            example.prefab

* **Click the "Help/Prepare for Missing scripts" menu.**

* Save and close the scene 

* Remove the behaviour meta file

    $ rm Assets/TestBehaviour.cs

* Remove the Library folder

    $ rm Library

* Open the scene

* Check a **MISSING SCRIPT** of example object

* **Click the "Help/Fix Missing scripts" menu.**

* Close and re-open the scene

* Re-check a **FIXED SCRIPT** of example object
