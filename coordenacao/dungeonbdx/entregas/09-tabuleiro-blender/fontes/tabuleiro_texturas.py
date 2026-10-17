# DungeonBDX — texturas do tabuleiro renderizadas no Blender (Claude, 16/10/2026).
# Uso: blender -b --factory-startup --python tabuleiro_texturas.py -- <pasta-saida> [ids...]
# Cada piso é um domínio 1x1 visto de cima (ortográfico, sem perspectiva). Peças que cruzam a borda
# são repetidas nos vizinhos, então a imagem emenda nas 4 bordas. A parede é uma elevação frontal
# que emenda na horizontal. Saída crua em baixa resolução; pos_pixel.py reduz a paleta e amplia.
import bpy, math, random, sys, os

ARGS = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
OUT = ARGS[0] if ARGS else os.path.join(os.path.dirname(__file__), '..', 'cru')
ONLY = set(ARGS[1:])
TILE_RES = 64          # pixels por casa (ampliado 8x depois -> 512)
os.makedirs(OUT, exist_ok=True)


def hexc(h, a=1.0):
    h = h.lstrip('#')
    srgb = [int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)]
    lin = [c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4 for c in srgb]
    return (*lin, a)


def reset(width, height, domain_w, domain_h, front=False):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    sc = bpy.context.scene
    sc.render.engine = 'CYCLES'
    sc.cycles.device = 'CPU'
    sc.cycles.samples = 96
    sc.cycles.use_denoising = True
    sc.cycles.filter_width = 0.01          # sem antisserrilhado: um raio no centro de cada pixel
    sc.render.resolution_x, sc.render.resolution_y = width, height
    sc.render.resolution_percentage = 100
    sc.render.film_transparent = False
    sc.view_settings.view_transform = 'Standard'
    sc.view_settings.look = 'None'
    world = bpy.data.worlds.new('mundo')
    world.use_nodes = True
    bg = world.node_tree.nodes['Background']
    bg.inputs[0].default_value = hexc('#c9c2b4')
    bg.inputs[1].default_value = 0.32
    sc.world = world
    cam_data = bpy.data.cameras.new('cam')
    cam_data.type = 'ORTHO'
    cam_data.ortho_scale = max(domain_w, domain_h)
    cam = bpy.data.objects.new('cam', cam_data)
    sc.collection.objects.link(cam)
    if front:
        cam.location = (domain_w / 2, -5, domain_h / 2)
        cam.rotation_euler = (math.radians(90), 0, 0)
    else:
        cam.location = (domain_w / 2, domain_h / 2, 5)
        cam.rotation_euler = (0, 0, 0)
    sc.camera = cam
    # Luz quase a pino, levemente do canto superior esquerdo: realça relevo sem sombra longa.
    sun_data = bpy.data.lights.new('sol', 'SUN')
    sun_data.energy = 4.2
    sun_data.angle = math.radians(18)
    sun = bpy.data.objects.new('sol', sun_data)
    sc.collection.objects.link(sun)
    if front:
        sun.rotation_euler = (math.radians(70), 0, math.radians(-25))
    else:
        sun.rotation_euler = (math.radians(-30), math.radians(-22), 0)
    return sc


MATS = {}


def mat(name, c1, c2=None, grit=0.25, grit_scale=60.0, rough=0.85, wave=None, metal=0.0):
    """Cor por objeto (aleatória entre c1 e c2) + granulado de ruído. wave=(escala, cor) faz veio."""
    if name in MATS:
        return MATS[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    nodes, links = nt.nodes, nt.links
    bsdf = nodes['Principled BSDF']
    bsdf.inputs['Roughness'].default_value = rough
    bsdf.inputs['Metallic'].default_value = metal
    info = nodes.new('ShaderNodeObjectInfo')
    ramp = nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color = hexc(c1)
    ramp.color_ramp.elements[1].color = hexc(c2 or c1)
    links.new(info.outputs['Random'], ramp.inputs['Fac'])
    noise = nodes.new('ShaderNodeTexNoise')
    noise.inputs['Scale'].default_value = grit_scale
    noise.inputs['Detail'].default_value = 3
    mix = nodes.new('ShaderNodeMix')
    mix.data_type = 'RGBA'
    mix.blend_type = 'MULTIPLY'
    mix.inputs['Factor'].default_value = grit
    links.new(ramp.outputs['Color'], mix.inputs['A'])
    links.new(noise.outputs['Color'], mix.inputs['B'])
    out = mix.outputs['Result']
    if wave:
        scale, vein = wave
        w = nodes.new('ShaderNodeTexWave')
        w.inputs['Scale'].default_value = scale
        w.inputs['Distortion'].default_value = 14
        w.inputs['Detail'].default_value = 4
        vr = nodes.new('ShaderNodeValToRGB')
        vr.color_ramp.elements[0].position = 0.0
        vr.color_ramp.elements[0].color = hexc(vein)
        vr.color_ramp.elements[1].position = 0.07
        vr.color_ramp.elements[1].color = (1, 1, 1, 1)
        links.new(w.outputs['Fac'], vr.inputs['Fac'])
        mix2 = nodes.new('ShaderNodeMix')
        mix2.data_type = 'RGBA'
        mix2.blend_type = 'MULTIPLY'
        mix2.inputs['Factor'].default_value = 1.0
        links.new(out, mix2.inputs['A'])
        links.new(vr.outputs['Color'], mix2.inputs['B'])
        out = mix2.outputs['Result']
    links.new(out, bsdf.inputs['Base Color'])
    MATS[name] = m
    return m


def _box(x, y, z, w, d, h, material, bevel, rot):
    bpy.ops.mesh.primitive_cube_add(size=1, location=(x, y, z + h / 2), rotation=(0, 0, rot))
    o = bpy.context.active_object
    o.scale = (w, d, h)
    bpy.ops.object.transform_apply(scale=True)
    if bevel > 0:
        b = o.modifiers.new('bevel', 'BEVEL')
        b.width = min(bevel, w / 2.2, d / 2.2, h / 2.2)
        b.segments = 3
        b.limit_method = 'NONE'
    o.data.materials.append(material)
    return o


def box(x, y, w, d, h, material, z=0.0, bevel=0.0, rot=0.0, wrap_x=True, wrap_y=True, dom_w=1.0, dom_h=1.0):
    """Caixa centrada em (x,y). Repete nos vizinhos quando cruza a borda (emenda perfeita)."""
    r = max(w, d) * 0.75
    for dx in ((-dom_w, 0, dom_w) if wrap_x else (0,)):
        for dy in ((-dom_h, 0, dom_h) if wrap_y else (0,)):
            cx, cy = x + dx, y + dy
            if cx + r < 0 or cx - r > dom_w or cy + r < 0 or cy - r > dom_h:
                continue
            _box(cx, cy, z, w, d, h, material, bevel, rot)


def disc(x, y, radius, h, material, z=0.0, verts=10):
    for dx in (-1, 0, 1):
        for dy in (-1, 0, 1):
            cx, cy = x + dx, y + dy
            if cx + radius < 0 or cx - radius > 1 or cy + radius < 0 or cy - radius > 1:
                continue
            bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=h, location=(cx, cy, z + h / 2))
            bpy.context.active_object.data.materials.append(material)


def ground(material, w=1.0, h=1.0, front=False):
    if front:
        bpy.ops.mesh.primitive_plane_add(size=1, location=(w / 2, 0.02, h / 2), rotation=(math.radians(90), 0, 0))
    else:
        bpy.ops.mesh.primitive_plane_add(size=1, location=(w / 2, h / 2, 0))
    o = bpy.context.active_object
    o.scale = (w * 3, h * 3, 1)
    o.data.materials.append(material)


def render(name, sc):
    sc.render.filepath = os.path.join(OUT, name + '.png')
    bpy.ops.render.render(write_still=True)
    print('render', name)


# ------------------------------------------------------------------ superfícies

def street(rng):
    ground(mat('rejunte-rua', '#3b3027', '#3b3027', grit=0.4))
    stone = mat('pedra-rua', '#a08a68', '#c9ad7b', grit=0.35)
    n = 4
    for j in range(n):
        off = 0.5 if j % 2 else 0.0
        for i in range(n):
            x = (i + off + 0.5 + rng.uniform(-0.08, 0.08)) / n
            y = (j + 0.5 + rng.uniform(-0.06, 0.06)) / n
            box(x, y, rng.uniform(0.17, 0.21), rng.uniform(0.17, 0.2), rng.uniform(0.05, 0.08), stone,
                bevel=0.05, rot=rng.uniform(-0.12, 0.12))


def crypt(rng):
    ground(mat('rejunte-cripta', '#1d1d22', '#1d1d22', grit=0.3))
    stone = mat('laje-cripta', '#6b6a6a', '#8d8880', grit=0.45, grit_scale=35)
    moss = mat('musgo', '#3f5a32', '#5c7a3f', grit=0.5)
    crack = mat('fenda', '#141416')
    # Lajes 2x2 com uma delas partida em duas.
    for j in range(2):
        for i in range(2):
            x0, y0 = i * 0.5, j * 0.5
            if (i, j) == (1, 0):
                box(x0 + 0.25, y0 + 0.13, 0.46, 0.21, 0.06, stone, bevel=0.025)
                box(x0 + 0.25, y0 + 0.37, 0.46, 0.21, 0.07, stone, bevel=0.025)
            else:
                box(x0 + 0.25, y0 + 0.25, 0.46, 0.46, rng.uniform(0.055, 0.075), stone, bevel=0.025)
    for _ in range(3):
        box(rng.uniform(0.1, 0.9), rng.uniform(0.1, 0.9), rng.uniform(0.12, 0.2), 0.012, 0.002, crack,
            z=0.07, rot=rng.uniform(0, math.pi))
    for _ in range(5):
        disc(rng.choice([0.0, 0.5, 1.0]) + rng.uniform(-0.04, 0.04), rng.uniform(0, 1), rng.uniform(0.02, 0.045), 0.01, moss, z=0.06)


def wood(rng):
    ground(mat('fresta-madeira', '#21160f', '#21160f', grit=0.2))
    plank = mat('tabua', '#7a4e2c', '#a26c3c', grit=0.2, wave=(6.0, '#5b3a22'))
    nail = mat('prego', '#2b2a28', '#3a3733', rough=0.4, metal=0.6)
    rows = 5
    for j in range(rows):
        y = (j + 0.5) / rows
        cut = rng.uniform(0.15, 0.85)
        lengths = [(cut, 1.0 - 0.02), (cut + 0.5, 0.0)]
        # Duas tábuas por fileira: a junta muda de lugar em cada fileira (emenda pela repetição).
        for start, _ in lengths[:1]:
            for k in range(2):
                a = (start + k * 0.5) % 1.0
                box(a + 0.25, y, 0.49, 1.0 / rows - 0.018, rng.uniform(0.05, 0.06), plank, bevel=0.012,
                    wrap_y=False)
                for side in (-1, 1):
                    disc((a + 0.02) % 1.0, y + side * 0.05, 0.009, 0.005, nail, z=0.06, verts=6)


def terracotta(rng):
    ground(mat('rejunte-terracota', '#c8b391', '#c8b391', grit=0.3))
    tile = mat('ladrilho-terracota', '#a9482b', '#cf6b3d', grit=0.3)
    n = 3
    for j in range(n):
        for i in range(n):
            box((i + 0.5) / n, (j + 0.5) / n, 1 / n - 0.03, 1 / n - 0.03, rng.uniform(0.045, 0.055), tile, bevel=0.018)


def mosaic(rng):
    ground(mat('rejunte-mosaico', '#d8cdb6', '#d8cdb6', grit=0.2))
    palette = {
        'c': mat('tessela-creme', '#e3d6b8', '#efe4c8', grit=0.15),
        't': mat('tessela-turquesa', '#1f6f6a', '#2f8c84', grit=0.15),
        'g': mat('tessela-ouro', '#c49a3c', '#ddb455', grit=0.15, rough=0.5, metal=0.3),
        'r': mat('tessela-terracota', '#a84a2e', '#c25d39', grit=0.15),
        'a': mat('tessela-azul', '#2a3f6b', '#34508a', grit=0.15),
    }
    n = 8
    for j in range(n):
        for i in range(n):
            cx, cy = i - 3.5, j - 3.5
            ring = max(abs(cx), abs(cy))
            diamond = abs(cx) + abs(cy)
            if ring == 3.5:
                k = 't'
            elif ring == 2.5:
                k = 'g' if (i + j) % 2 else 'c'
            elif diamond <= 1:
                k = 'r'
            elif diamond <= 2:
                k = 'a'
            else:
                k = 'c'
            box((i + 0.5) / n + rng.uniform(-0.006, 0.006), (j + 0.5) / n + rng.uniform(-0.006, 0.006),
                1 / n - 0.022, 1 / n - 0.022, rng.uniform(0.03, 0.04), palette[k], bevel=0.008,
                rot=rng.uniform(-0.06, 0.06))


def marble(rng):
    ground(mat('junta-marmore', '#9c7b3a', '#9c7b3a', rough=0.4, metal=0.5))
    slab = mat('marmore', '#ddd8ce', '#f1ede4', grit=0.04, rough=0.35, wave=(1.6, '#a39c92'))
    for j in range(2):
        for i in range(2):
            box(i * 0.5 + 0.25, j * 0.5 + 0.25, 0.48, 0.48, 0.05, slab, bevel=0.01)


def garden(rng):
    ground(mat('grama', '#3f6a2c', '#3f6a2c', grit=0.55, grit_scale=12))
    blade = mat('folha', '#5c8f3c', '#8bb853', grit=0.2)
    stone = mat('pedra-jardim', '#9a9184', '#b5ab9a', grit=0.35)
    flowers = [mat('flor-amarela', '#e2c249'), mat('flor-branca', '#ece6da'), mat('flor-lilas', '#9a6cc0')]
    box(0.5, 0.5, 0.36, 0.3, 0.05, stone, bevel=0.06, rot=0.3)
    for _ in range(55):
        x, y = rng.random(), rng.random()
        if abs(x - 0.5) < 0.2 and abs(y - 0.5) < 0.18:
            continue
        box(x, y, rng.uniform(0.04, 0.07), rng.uniform(0.03, 0.05), rng.uniform(0.03, 0.06), blade,
            rot=rng.uniform(0, math.pi))
    for _ in range(7):
        x, y = rng.random(), rng.random()
        if abs(x - 0.5) < 0.22 and abs(y - 0.5) < 0.2:
            continue
        disc(x, y, 0.014, 0.012, rng.choice(flowers), z=0.04, verts=6)


WALL_H = 192 / 512


def wall(rng):
    # Elevação frontal: blocos de pedra em amarração corrida, emenda horizontal.
    ground(mat('argamassa', '#262120', '#262120', grit=0.35), w=1.0, h=WALL_H, front=True)
    stone = mat('bloco-parede', '#6a5f53', '#a39280', grit=0.4, grit_scale=30)
    rows = 3
    bh = WALL_H / rows
    for j in range(rows):
        cols = 3
        off = 0.5 / cols if j % 2 else 0.0
        for i in range(cols):
            x = (i + 0.5) / cols + off
            z = (j + 0.5) * bh
            for dx in (-1, 0, 1):
                cx = x + dx
                if cx + 0.2 < 0 or cx - 0.2 > 1:
                    continue
                bpy.ops.mesh.primitive_cube_add(size=1, location=(cx, -rng.uniform(0.01, 0.025), z))
                o = bpy.context.active_object
                o.scale = (1 / cols - 0.018, 0.05, bh - 0.016)
                bpy.ops.object.transform_apply(scale=True)
                b = o.modifiers.new('bevel', 'BEVEL')
                b.width, b.segments = 0.008, 2
                o.data.materials.append(stone)


def frame(rng):
    # Moldura da mesa (vista de cima, emenda nas 4 bordas): pedra escura com friso de bronze.
    ground(mat('pedra-moldura', '#3c342c', '#3c342c', grit=0.4, grit_scale=30))
    block = mat('bloco-moldura', '#4f4438', '#625443', grit=0.35, grit_scale=30)
    bronze = mat('friso-bronze', '#8a6a34', '#a07c3e', grit=0.15, rough=0.45, metal=0.6)
    for j in range(2):
        for i in range(2):
            box(i * 0.5 + 0.25, j * 0.5 + 0.25, 0.47, 0.47, 0.05, block, bevel=0.02)
    for t in (0.0, 0.5):
        box(0.5, t, 1.0, 0.04, 0.07, bronze, bevel=0.01, wrap_x=False)
        box(t, 0.5, 0.04, 1.0, 0.07, bronze, bevel=0.01, wrap_y=False)
    for x in (0.0, 0.5):
        for y in (0.0, 0.5):
            disc(x, y, 0.045, 0.08, bronze, verts=8)


SURFACES = [
    ('street', street), ('crypt', crypt), ('wood', wood), ('terracotta', terracotta),
    ('mosaic', mosaic), ('marble', marble), ('garden', garden), ('frame', frame),
]

for i, (name, build) in enumerate(SURFACES):
    if ONLY and name not in ONLY:
        continue
    MATS.clear()
    sc = reset(TILE_RES, TILE_RES, 1.0, 1.0)
    build(random.Random(1600 + i))
    render(name, sc)

if not ONLY or 'wall' in ONLY:
    MATS.clear()
    sc = reset(TILE_RES, int(TILE_RES * WALL_H), 1.0, WALL_H, front=True)
    wall(random.Random(1700))
    render('wall', sc)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, 'wall.blend'))
