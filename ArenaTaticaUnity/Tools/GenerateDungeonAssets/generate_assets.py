"""Original DungeonBDX kit. Run with Blender 5.2 --background --python this_file.

No downloads, third-party meshes, image textures or commercial board-game tiles.
The source geometry uses Blender Z-up; FBX exports Unity Y-up at metre scale.
"""
import bpy
import math
import json
import hashlib
import random
import struct
import wave
from pathlib import Path
from mathutils import Vector

PROJECT = Path(__file__).resolve().parents[2]
ART = PROJECT / 'Assets' / 'DungeonBDX' / 'Art'
RES = ART / 'Resources' / 'DungeonBDX'
MODELS = RES / 'Models'
SOURCE = PROJECT / 'Tools' / 'GenerateDungeonAssets' / 'Source'
for folder in (MODELS, SOURCE, RES / 'Audio' / 'Music', RES / 'Audio' / 'Sfx'):
    folder.mkdir(parents=True, exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
PALETTE = {
    'slate': ((0.095, 0.12, 0.16, 1), 0.1),
    'stone': ((0.18, 0.20, 0.24, 1), 0.05),
    'bronze': ((0.48, 0.26, 0.09, 1), 0.72),
    'gold': ((0.80, 0.52, 0.17, 1), 0.7),
    'wine': ((0.34, 0.055, 0.12, 1), 0.05),
    'cloth': ((0.06, 0.065, 0.10, 1), 0.0),
    'skin': ((0.55, 0.31, 0.22, 1), 0.0),
    'ivory': ((0.72, 0.68, 0.50, 1), 0.05),
    'aqua': ((0.07, 0.62, 0.72, 1), 0.35),
    'violet': ((0.34, 0.12, 0.56, 1), 0.15),
    'ember': ((1.0, 0.29, 0.045, 1), 0.0),
    'green': ((0.15, 0.37, 0.27, 1), 0.12),
}
MATS = {}
for name, (color, metallic) in PALETTE.items():
    mat = bpy.data.materials.new('DBDX_' + name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = color
    bsdf.inputs['Metallic'].default_value = metallic
    bsdf.inputs['Roughness'].default_value = 0.56
    if name in ('aqua', 'ember', 'violet'):
        bsdf.inputs['Emission Color'].default_value = color
        bsdf.inputs['Emission Strength'].default_value = 0.35
    MATS[name] = mat

CURRENT = None
OBJECTS = []
MANIFEST = []

def own(obj, name, material):
    obj.name = name
    for col in list(obj.users_collection):
        col.objects.unlink(obj)
    CURRENT.objects.link(obj)
    obj.data.materials.append(MATS[material])
    OBJECTS.append(obj)
    return obj

def cube(name, p, size, mat='stone', bevel=0.0, rotation=None):
    bpy.ops.mesh.primitive_cube_add(size=1, location=p)
    o = own(bpy.context.object, name, mat)
    o.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if rotation:
        o.rotation_euler = rotation
    if bevel:
        mod = o.modifiers.new('Original edge chamfer', 'BEVEL')
        mod.width = bevel
        mod.segments = 1
        bpy.context.view_layer.objects.active = o
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return o

def cone(name, p, radius, depth, mat='bronze', vertices=8, top=0):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius,
                                  radius2=top, depth=depth, location=p)
    return own(bpy.context.object, name, mat)

def ico(name, p, radius, mat='stone', scale=None):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=radius, location=p)
    o = own(bpy.context.object, name, mat)
    if scale:
        o.scale = scale
    return o

def rod(name, start, end, radius, mat='bronze', vertices=8):
    delta = Vector(end) - Vector(start)
    o = cone(name, (Vector(start) + Vector(end)) / 2, radius, delta.length,
             mat, vertices, radius)
    o.rotation_euler = delta.to_track_quat('Z', 'Y').to_euler()
    return o

def mesh(name, verts, faces, mat):
    m = bpy.data.meshes.new(name)
    m.from_pydata(verts, [], faces)
    m.update()
    o = bpy.data.objects.new(name, m)
    CURRENT.objects.link(o)
    o.data.materials.append(MATS[mat])
    OBJECTS.append(o)
    return o

def begin(name):
    global CURRENT, OBJECTS
    OBJECTS = []
    CURRENT = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(CURRENT)

def finish(name, kind, scenarios, collider, ports=None):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in OBJECTS:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = OBJECTS[0]
    for o in OBJECTS:
        o.data.calc_loop_triangles()
    triangles = sum(len(o.data.loop_triangles) for o in OBJECTS)
    world = [o.matrix_world @ Vector(corner) for o in OBJECTS for corner in o.bound_box]
    low = [min(v[i] for v in world) for i in range(3)]
    high = [max(v[i] for v in world) for i in range(3)]
    path = MODELS / (name + '.fbx')
    bpy.ops.export_scene.fbx(filepath=str(path), use_selection=True,
        object_types={'MESH'}, axis_forward='-Z', axis_up='Y',
        global_scale=1.0, apply_unit_scale=True, bake_anim=False,
        add_leaf_bones=False, use_mesh_modifiers=True, mesh_smooth_type='FACE',
        path_mode='AUTO', use_custom_props=False)
    MANIFEST.append({
        'id': 'dbdx.' + ('hero.' if kind == 'hero' else 'model.') + name,
        'name': name, 'kind': kind, 'resource': 'DungeonBDX/Models/' + name,
        'path': str(path.relative_to(ART)).replace('\\', '/'),
        'triangles': triangles, 'meshCount': len(OBJECTS),
        'unityBoundsMeters': {'min': [round(low[0], 4), round(low[2], 4), round(-high[1], 4)],
                              'max': [round(high[0], 4), round(high[2], 4), round(-low[1], 4)]},
        'pivot': 'floor center at origin', 'colliderRecommendation': collider,
        'scenarios': scenarios, 'ports': ports or [],
        'animation': 'static model; runtime transform motion only; skeletal clips pending',
    })
    # Keep an editable source library, separated on a catalogue grid after export.
    index = len(MANIFEST) - 1
    for obj in OBJECTS:
        obj.location.x += (index % 6) * 3.0
        obj.location.y += (index // 6) * 3.0

def tile(name, theme):
    begin(name)
    cube('foundation', (0, 0, -0.085), (1, 1, .17), 'slate', .025)
    for x in (-.245, .245):
        for y in (-.245, .245):
            cube('inset flagstone', (x, y, -.005), (.46, .46, .035), theme, .015)
    for x in (-.475, .475):
        cube('border rail', (x, 0, .005), (.025, .94, .035), 'bronze')
    if theme == 'violet':
        for a in range(4):
            cube('arcane inlay', (0, 0, .018), (.55, .018, .006), 'aqua',
                 rotation=(0, 0, a * math.pi / 4))
    if theme == 'green':
        for a in range(5):
            angle = a * math.tau / 5
            rod('nest root', (.1 * math.cos(angle), .1 * math.sin(angle), .03),
                (.4 * math.cos(angle + .3), .4 * math.sin(angle + .3), .025), .018, 'bronze', 5)
    finish(name, 'floor', ['all'], 'BoxCollider 1 x 0.17 x 1; floor top 0', ['N', 'E', 'S', 'W'])

tile('floor-bronze', 'stone')
tile('floor-library', 'violet')
tile('floor-nest', 'green')

begin('wall-bronze')
cube('wall body', (0, 0, .5), (1, .20, 1), 'slate', .025)
for z in (.12, .38, .64, .9):
    for x in (-.255, .255):
        cube('masonry', (x, -.115, z), (.47, .06, .22), 'stone', .012)
cube('top cornice', (0, 0, 1.02), (1.04, .28, .10), 'bronze', .02)
finish('wall-bronze', 'wall', ['all'], 'BoxCollider')

begin('door-arch')
for x in (-.4, .4):
    cube('jamb', (x, 0, .56), (.2, .27, 1.12), 'stone', .025)
    cube('jamb base', (x, 0, .09), (.26, .34, .18), 'bronze', .018)
cube('lintel', (0, 0, 1.2), (1.03, .30, .25), 'stone', .04)
ico('keystone', (0, -.18, 1.22), .13, 'gold', (1, .45, 1.2))
finish('door-arch', 'door-frame', ['all'], '2 side BoxColliders, open center', ['N', 'S'])

begin('column-bronze')
cone('base', (0, 0, .08), .26, .16, 'bronze', 8, .26)
cone('shaft', (0, 0, .66), .14, 1.03, 'stone', 8, .12)
for z in (.24, 1.04):
    cone('collar', (0, 0, z), .18, .09, 'bronze', 8, .18)
cone('capital', (0, 0, 1.22), .25, .16, 'bronze', 8, .25)
finish('column-bronze', 'prop', ['cripta-de-bronze'], 'BoxCollider')

begin('bookcase')
cube('back panel', (0, .12, .68), (.76, .08, 1.36), 'wine', .015)
for x in (-.38, .38):
    cube('side', (x, 0, .7), (.07, .32, 1.4), 'bronze', .015)
for z in (.05, .45, .85, 1.28):
    cube('shelf', (0, 0, z), (.82, .36, .07), 'bronze', .012)
for row, z in enumerate((.24, .64, 1.06)):
    for i in range(7):
        height = .24 + (i % 3) * .025
        cube('bound tome', (-.3 + i * .1, -.035, z), (.065, .18, height),
             ('wine', 'violet', 'ivory', 'green')[(i + row) % 4], .005)
finish('bookcase', 'prop', ['biblioteca-do-crepusculo'], 'BoxCollider')

begin('brazier')
cone('pedestal', (0, 0, .19), .16, .38, 'bronze', 8, .10)
cone('bowl', (0, 0, .43), .12, .14, 'bronze', 8, .24)
for i in range(5):
    a = i * math.tau / 5
    cone('ember tongue', (.07 * math.cos(a), .07 * math.sin(a), .62), .08, .34,
         'ember', 5)
finish('brazier', 'prop', ['cripta-de-bronze'], 'SphereCollider lower pedestal')

begin('nest-spire')
for i, (x, y, z) in enumerate(((-.14, 0, .50), (.16, .05, .32), (0, -.1, .7))):
    o = cone('basalt shard', (x, y, z), .16, z * 2, 'slate', 5)
    o.rotation_euler = (.1 * (i - 1), .14 * (i - 1), i)
    ico('eclipse core', (x, y, z * 1.65), .065, 'violet')
finish('nest-spire', 'prop', ['ninho-do-eclipse'], 'BoxCollider')

begin('dragon-egg')
for i in range(9):
    a = i * math.tau / 9
    rod('nest woven branch', (.34 * math.cos(a), .34 * math.sin(a), .06),
        (.34 * math.cos(a + 1.4), .34 * math.sin(a + 1.4), .06), .035, 'bronze', 5)
ico('eggshell', (0, 0, .27), .22, 'ivory', (.8, .8, 1.3))
for i in range(4):
    a = i * math.tau / 4
    ico('shell sigil', (.15 * math.cos(a), .15 * math.sin(a), .30), .035, 'violet')
finish('dragon-egg', 'objective-prop', ['ninho-do-eclipse'], 'SphereCollider')

begin('objective-crystal')
cone('plinth', (0, 0, .08), .23, .16, 'bronze', 8, .23)
o = ico('floating focus', (0, 0, .56), .26, 'aqua', (.65, .65, 1.4))
for i in range(4):
    a = i * math.tau / 4
    rod('rune prong', (.2 * math.cos(a), .2 * math.sin(a), .13),
        (.23 * math.cos(a), .23 * math.sin(a), .42), .018, 'gold', 5)
finish('objective-crystal', 'objective-prop', ['all'], 'SphereCollider')

begin('crate')
cube('wood body', (0, 0, .23), (.46, .46, .46), 'wine', .025)
for x in (-.20, .20):
    cube('binding', (x, 0, .23), (.045, .49, .49), 'bronze', .008)
finish('crate', 'prop', ['all'], 'BoxCollider')

begin('rubble')
for i in range(7):
    a = i * math.tau / 7
    ico('broken stone', (.27 * math.cos(a), .24 * math.sin(a), .08), .11 + .02 * (i % 3),
        'stone', (1.2, .8, .55))
finish('rubble', 'prop', ['all'], 'No collider; decorative')

begin('trap-spikes')
cube('pressure plate', (0, 0, .025), (.7, .7, .05), 'slate', .025)
for x in (-.22, 0, .22):
    for y in (-.22, 0, .22):
        cone('spike', (x, y, .16), .055, .27, 'ivory', 5)
finish('trap-spikes', 'prop', ['all'], 'Trigger BoxCollider only; rules authority external')

def body(name, cloak, hat=False):
    # Original adult stylized silhouettes; no copied Arena or AQ meshes.
    for x in (-.105, .105):
        cube('boot', (x, -.015, .11), (.15, .25, .22), 'cloth', .025)
        rod('leg', (x, 0, .22), (x, 0, .67), .073, 'slate')
    cone('coat', (0, .025, .73), .24, .48, cloak, 8, .16)
    cube('torso', (0, 0, .99), (.33, .22, .38), cloak, .035)
    cube('belt', (0, -.005, .81), (.36, .25, .055), 'bronze', .006)
    ico('head', (0, -.005, 1.35), .15, 'skin', (.85, .9, 1.13))
    cone('hair', (0, .025, 1.44), .145, .12, 'cloth', 8, .11)
    if hat:
        cone('hat brim', (0, 0, 1.49), .25, .035, 'cloth', 10, .25)
        cone('hat crown', (0, .02, 1.58), .13, .17, 'cloth', 8, .11)

begin('lyra-faisca')
body('Lyra', 'wine', True)
for side in (-1, 1):
    rod('sleeve', (side * .19, 0, 1.1), (side * .3, -.08, .88), .075, 'wine')
    rod('forearm', (side * .3, -.08, .88), (side * .31, -.24, .98), .052, 'skin')
    cube('sidearm receiver', (side * .31, -.29, 1.015), (.08, .18, .075), 'bronze', .01)
    rod('sidearm barrel', (side * .31, -.34, 1.03), (side * .31, -.49, 1.03), .027, 'gold')
cube('scarf', (0, -.14, 1.19), (.21, .04, .14), 'gold', .01)
finish('lyra-faisca', 'hero', ['all'], 'CapsuleCollider; upright footprint <= 0.9 m')

begin('orion-prisma')
body('Orion', 'violet')
cone('hood', (0, .045, 1.4), .21, .30, 'violet', 6, .06)
cube('hood face opening', (0, -.126, 1.36), (.16, .025, .15), 'cloth', .015)
for side in (-1, 1):
    rod('sleeve', (side * .19, 0, 1.1), (side * .31, -.04, .90), .09, 'violet')
    ico('glove', (side * .31, -.06, .88), .065, 'bronze')
rod('staff', (.35, -.07, .10), (.35, -.07, 1.55), .026, 'gold')
ico('staff prism', (.35, -.07, 1.65), .13, 'aqua', (.65, .65, 1.2))
for z in (.59, .72, .86, 1.0):
    ico('robe rune', (0, -.16, z), .03, 'aqua', (1, .3, 1))
finish('orion-prisma', 'hero', ['all'], 'CapsuleCollider')

begin('vaelis-vinculo')
body('Vaelis', 'green')
for side in (-1, 1):
    ico('scale pauldron', (side * .21, 0, 1.13), .13, 'bronze', (1.1, .9, .7))
    rod('sleeve', (side * .2, 0, 1.08), (side * .33, -.08, .88), .07, 'green')
    ico('hand', (side * .33, -.08, .85), .055, 'skin')
    cone('horn crown', (side * .12, .02, 1.53), .045, .25, 'ivory', 5)
rod('binding crook', (.34, -.10, .1), (.34, -.1, 1.36), .025, 'bronze')
ico('bond focus', (.34, -.1, 1.42), .10, 'violet')
cube('dragon-bond chest medallion', (0, -.15, 1.04), (.13, .045, .12), 'gold', .015)
finish('vaelis-vinculo', 'hero', ['all'], 'CapsuleCollider')

begin('companion-dragon')
ico('body', (0, 0, .25), .25, 'green', (.8, 1.3, .8))
ico('neck', (0, -.26, .40), .15, 'green', (.8, .8, 1.4))
ico('head', (0, -.36, .50), .14, 'green', (1, 1.3, .8))
ico('muzzle', (0, -.48, .47), .10, 'bronze', (.8, 1.1, .6))
for side in (-1, 1):
    ico('eye', (side * .092, -.4, .54), .022, 'ember')
    rod('horn', (side * .07, -.29, .57), (side * .11, -.20, .72), .027, 'ivory', 5)
    for y in (-.16, .17):
        rod('leg', (side * .14, y, .24), (side * .21, y - .04, .07), .045, 'green', 6)
        cube('claw foot', (side * .21, y - .07, .045), (.10, .15, .065), 'ivory', .012)
    verts = [(side * .10, .02, .38), (side * .56, -.17, .63),
             (side * .50, .20, .39), (side * .29, .32, .30)]
    mesh('wing membrane', verts, [(0, 1, 2), (0, 2, 3), (2, 1, 0), (3, 2, 0)], 'wine')
    for end in verts[1:]:
        rod('wing finger', verts[0], end, .012, 'bronze', 5)
rod('tail', (0, .23, .25), (.10, .54, .15), .065, 'green', 6)
rod('tail tip', (.10, .54, .15), (.20, .69, .23), .03, 'bronze', 5)
finish('companion-dragon', 'companion', ['all'], 'CapsuleCollider; wide wings are visual only')

bpy.context.scene.unit_settings.system = 'METRIC'
bpy.context.scene.unit_settings.scale_length = 1.0
bpy.context.scene.render.engine = 'CYCLES'
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE / 'DungeonBDX-original-kit.blend'), compress=True)

# Original deterministic additive-synthesis loops. Frequencies quantized to whole
# periods so samples, modulators and equal-power panning wrap at the loop boundary.
RATE = 22050
def write_wave(path, samples, stereo=False):
    frames = bytearray()
    peak = 0.0
    square_sum = 0.0
    for sample in samples:
        channels = sample if stereo else (sample,)
        for value in channels:
            peak = max(peak, abs(value))
            square_sum += value * value
            frames.extend(struct.pack('<h', round(max(-1, min(1, value)) * 32767)))
    with wave.open(str(path), 'wb') as f:
        f.setnchannels(2 if stereo else 1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes(frames)
    count = len(frames) // (4 if stereo else 2)
    return {'durationSeconds': count / RATE, 'sampleRate': RATE,
            'channels': 2 if stereo else 1, 'peakLinear': round(peak, 5),
            'rmsLinear': round(math.sqrt(square_sum / (count * (2 if stereo else 1))), 5),
            'loop': stereo, 'encoding': 'PCM signed 16-bit little endian'}

MUSIC = [
    ('cripta-de-bronze', (55, 82.4069, 110, 130.8128), 16, 'bronze sub-drone and soft metallic overtones'),
    ('biblioteca-do-crepusculo', (73.4162, 110, 146.8324, 174.6141), 16, 'airy sustained suspended harmony and glass-like arpeggio'),
    ('ninho-do-eclipse', (41.2034, 61.7354, 82.4069, 123.4708), 16, 'low breath-like drone and slow dissonant harmonic pulse'),
]
audio_entries = []
for index, (name, notes, seconds, description) in enumerate(MUSIC):
    quantized = [round(f * seconds) / seconds for f in notes]
    def ambient():
        for i in range(seconds * RATE):
            t = i / RATE
            left = right = 0.0
            for j, f in enumerate(quantized):
                amp = (.65 + .35 * math.sin(math.tau * (j + 1) * t / seconds + j))
                tone = math.sin(math.tau * f * t + j * .6)
                tone += .20 * math.sin(math.tau * f * 2 * t + j)
                tone += .055 * math.sin(math.tau * f * (3 + index) * t)
                pan = .5 + .32 * math.sin(math.tau * t / seconds + j)
                left += .055 * tone * amp * math.sqrt(1 - pan)
                right += .055 * tone * amp * math.sqrt(pan)
            # Seamless bright sequence: continuous periodic envelope, not a sample splice.
            bell_freq = quantized[(int(t / 2) + index) % len(quantized)] * 4
            # Reset only when envelope and its first derivative are zero.
            phase = (t % 2) / 2
            env = math.sin(math.pi * phase) ** 4
            bell = .018 * env * math.sin(math.tau * bell_freq * (t % 2))
            yield (left + bell, right + bell)
    path = RES / 'Audio' / 'Music' / (name + '.wav')
    info = write_wave(path, ambient(), True)
    audio_entries.append({'id': 'dbdx.music.' + name,
        'resource': 'DungeonBDX/Audio/Music/' + name, 'kind': 'music-prototype',
        'path': str(path.relative_to(ART)).replace('\\', '/'), 'description': description, **info})

SFX = {
    'basico.golpe': ('hit', .19), 'basico.avanco': ('whoosh', .32),
    'basico.revigorar': ('heal', .65), 'basico.baluarte': ('shield', .5),
    'pistola.duplo': ('gun', .20), 'pistola.ricochete': ('gun', .35),
    'pistola.mira': ('aim', .38), 'pistola.ruptura': ('gun', .40),
    'arcano.lanca': ('magic', .48), 'arcano.geada': ('ice', .65),
    'arcano.egide': ('shield', .55), 'arcano.transposicao': ('whoosh', .45),
    'arcano.pulso': ('heal', .75), 'draco.invocar': ('summon', .85),
    'draco.comandar': ('hit', .26), 'draco.sopro': ('fire', .75),
    'draco.pacto': ('shield', .65), 'draco.recolher': ('heal', .50),
}
for number, (skill, (kind, duration)) in enumerate(SFX.items()):
    rng = random.Random(8127 + number)
    total = int(RATE * duration)
    def sound():
        previous = 0.0
        for i in range(total):
            t, p = i / RATE, i / total
            noise = rng.uniform(-1, 1)
            previous = previous * .75 + noise * .25
            fade = min(1, p * 60) * min(1, (1 - p) * 25)
            if kind == 'gun':
                value = (.23 * noise * math.exp(-t * 48)
                         + .19 * math.sin(math.tau * (95 * t - 55 * t*t)) * math.exp(-t * 15))
                if skill.endswith('duplo') and t > .085:
                    value += .17 * noise * math.exp(-(t - .085) * 60)
            elif kind in ('hit', 'fire', 'whoosh', 'ice'):
                speed = {'hit': 19, 'fire': 2.2, 'whoosh': 5, 'ice': 6}[kind]
                value = .23 * previous * math.sin(math.pi * p) ** .6 * math.exp(-t * speed)
                if kind == 'ice':
                    value += .075 * math.sin(math.tau * (1500 * t + 220 * t*t)) * math.exp(-t * 9)
            else:
                f = {'heal': 440, 'shield': 220, 'aim': 760, 'magic': 330, 'summon': 110}[kind]
                phase = math.tau * (f * t + f * .45 * t*t)
                value = (.12 * math.sin(phase) + .035 * math.sin(phase * 1.5)) * math.sin(math.pi * p) ** 1.6
            yield value * fade
    name = skill.replace('.', '-')
    path = RES / 'Audio' / 'Sfx' / (name + '.wav')
    info = write_wave(path, sound())
    audio_entries.append({'id': 'dbdx.sfx.' + skill,
        'resource': 'DungeonBDX/Audio/Sfx/' + name, 'kind': 'skill-sfx-prototype',
        'path': str(path.relative_to(ART)).replace('\\', '/'), **info})

for entry in MANIFEST + audio_entries:
    path = ART / entry['path']
    entry['bytes'] = path.stat().st_size
    entry['sha256'] = hashlib.sha256(path.read_bytes()).hexdigest()
source = SOURCE / 'DungeonBDX-original-kit.blend'
manifest = {
    'version': '0.1.0', 'status': 'original functional art/audio prototype; final high-end art and soundtrack pending',
    'author': 'DungeonBDX asset production via Codex / Blender procedural geometry and Python synthesis',
    'provenance': 'Created from first principles for this project. No downloaded or copied AQ tiles, meshes, samples or tracks.',
    'license': 'Project-original assets; reusable by the project author. Generator source included. No third-party asset license required.',
    'units': '1 Unity unit = 1 metre; Y up, forward +Z; pivot at floor center',
    'fbxImport': 'Static meshes with diffuse material colours, no textures. Unity URP consumer must use URP/Lit or compatible shader. Collider recommendations are metadata, not baked physics.',
    'budget': {'totalMeasuredTriangles': sum(x['triangles'] for x in MANIFEST),
               'modelCount': len(MANIFEST), 'musicLoops': len(MUSIC), 'skillSounds': len(SFX),
               'runtimeGpuOrMobileProfiling': 'not measured; author QA pending'},
    'source': {'path': str(source.relative_to(PROJECT)).replace('\\', '/'), 'bytes': source.stat().st_size,
               'sha256': hashlib.sha256(source.read_bytes()).hexdigest()},
    'models': MANIFEST, 'audio': audio_entries,
    'pending': ['Skeletal rigs and animation clips', 'High-end sculpting, authored textures and LODs',
                'Final composed scenario soundtracks', '31 hero card templates',
                'Adapted meshes for the 28 original Arena hero identities',
                'Reference image palette verification', 'Physical PC/mobile performance and stress QA'],
}
(ART / 'manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print('DUNGEON_ASSETS_OK ' + json.dumps(manifest['budget']))
