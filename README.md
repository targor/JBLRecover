# JBLRecover
ensures that JBL headphones (at least for my Quantum 800) do not go into sleep mode

# Screenshot:

![preview](https://user-images.githubusercontent.com/7933943/144284705-0c00eef6-b444-48bd-93b9-e073902373bb.png)

# Why?

I got a new JBL headset and i am very happy with it but one thing. When i am doing online meetings or stopping my music because i need some more concentration, then the headset will shutdown after about 10 minues. This really suucks. JBL  support stated, that this can not be changed, so i wrote this program, which will keep the headset stying awake.

# How is it done?

The application plays a wave file with 16196Hz which should not be hearyble by human ears, but is recognized by the headphone. The play duration is about 5 seconds and the sound will adapt to the current mastervolume, so that it wont be played too loud to prevent damage to the ear. The sound is also only played when there currently is no soundoutput playing from another sources.

By doing that, the headset will not go into sleep mode. The default time is set to 9 minutes (as the JBL headphones will shutdown after about 10 minutes of silence), but it can be reduced if wanted.

# Features:

- plays 16196Hz unhearble soundfile
- time adjustable
- adjusts play volume automatically depending on master volume
- only play, when there is no other source making sounds
- runs in the background :D


