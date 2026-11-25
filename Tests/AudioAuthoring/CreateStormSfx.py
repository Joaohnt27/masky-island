"""Sopro original sintetizado; sem gravações ou dependências externas."""
import math, random, struct, wave
from pathlib import Path
destination = Path(__file__).resolve().parents[2] / 'Assets/Resources/CartoonSfx/Storm.wav'
rng = random.Random(1703)
rate = 44100
duration = 2.2
filtered = 0.0
frames = bytearray()
for i in range(int(rate * duration)):
    t = i / rate
    envelope = math.sin(math.pi * t / duration) ** 2
    filtered = .96 * filtered + .04 * rng.uniform(-1, 1)
    tone = math.sin(2 * math.pi * (125 * t - 13 * t * t))
    sample = envelope * (.6 * filtered + .07 * tone)
    frames.extend(struct.pack('<h', int(max(-1, min(1, sample)) * 32767)))
with wave.open(str(destination), 'wb') as audio:
    audio.setnchannels(1); audio.setsampwidth(2); audio.setframerate(rate)
    audio.writeframes(frames)
