"""Original synthesized cartoon cues, mono PCM WAV. No external recordings."""
from pathlib import Path
import math, random, wave, struct
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/Resources/CartoonSfx'; OUT.mkdir(parents=True,exist_ok=True)
SR=44100
rng=random.Random(528)
def tone(freq,t): return math.sin(2*math.pi*freq*t)+.22*math.sin(4*math.pi*freq*t)
def notes(t, seq, beat):
    i=int(t/beat)
    if i>=len(seq): return 0
    local=t-i*beat
    return tone(seq[i],local)*math.exp(-local*14)*min(1,local/.006)
def synth(name,duration,gain):
    result=[]
    for i in range(int(SR*duration)):
        t=i/SR; u=t/duration
        attack=min(1,t/.004); fade=min(1,(duration-t)/.018)
        if name=='Click': value=math.sin(2*math.pi*(850*t-1800*t*t))*math.exp(-t*48)
        elif name=='Open': value=math.sin(2*math.pi*(350*t+1300*t*t))*math.sin(math.pi*u)**2
        elif name=='Close': value=math.sin(2*math.pi*(650*t-1500*t*t))*math.sin(math.pi*u)**2
        elif name=='Pickaxe': value=(.65*tone(310,t)+.35*rng.uniform(-1,1))*math.exp(-t*34)
        elif name=='Crystal': value=notes(t,[1046.5,1318.5,1568,2093],.13)
        elif name=='Success': value=notes(t,[523.25,659.25,783.99,1046.5],.18)
        elif name=='Error': value=math.sin(2*math.pi*(210*t-180*t*t)+2*math.sin(2*math.pi*22*t))*math.exp(-t*10)
        elif name=='Confirm': value=tone(1108.7,t)*math.exp(-t*27)
        elif name=='Travel': value=(.5*math.sin(2*math.pi*(180*t+1300*t*t))+.18*rng.uniform(-1,1))*math.sin(math.pi*u)**2
        elif name=='Beacon': value=sum(tone(f,t) for f in [261.63,329.63,392,523.25])/4*math.sin(math.pi*u)**.6
        else: value=(.8*tone(100,t)+.2*rng.uniform(-1,1))*math.exp(-t*62)
        result.append(value*attack*fade)
    peak=max(abs(x) for x in result)
    samples=[x/peak*gain for x in result]
    with wave.open(str(OUT/(name+'.wav')),'wb') as w:
        w.setparams((1,2,SR,0,'NONE','not compressed'))
        w.writeframes(b''.join(struct.pack('<h',round(x*32767)) for x in samples))
    return samples
cues=[('Click',.09,.35),('Open',.22,.36),('Close',.16,.3),('Pickaxe',.17,.5),('Crystal',.65,.5),('Success',.9,.5),('Error',.35,.4),('Confirm',.13,.35),('Travel',.42,.35),('Beacon',1.4,.55),('Footstep',.08,.18)]
demo=[]
for args in cues:
    samples=synth(*args);demo+=samples+[0.0]*int(SR*.25)
folder=ROOT/'Documentation/MapAudio';folder.mkdir(parents=True,exist_ok=True)
with wave.open(str(folder/'CartoonSfxPreview.wav'),'wb') as w:
    w.setparams((1,2,SR,0,'NONE','not compressed'));w.writeframes(b''.join(struct.pack('<h',round(x*32767)) for x in demo))
print('Created',len(cues),'original clips; maximum peak 0.55; no clipping.')
