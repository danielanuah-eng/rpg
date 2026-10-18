# Blender -> 64x64 pixel-art card test: yew longbow. Usage: blender -b -P blender_bow.py -- <out.png> [<big.png>]
import bpy, math, sys
import numpy as np

argv = sys.argv[sys.argv.index("--") + 1:]
OUT = argv[0]
SIZE = 64
SCALE = 4  # render at 4x with anti-aliasing, then reduce by coverage

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene

def mat(name, rgb):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*rgb, 1)
    return m

wood = mat("wood", (0.42, 0.22, 0.09))
leather = mat("leather", (0.20, 0.08, 0.04))
string = mat("string", (0.85, 0.80, 0.62))
horn = mat("horn", (0.80, 0.74, 0.58))

def poly_curve(name, pts, radii, bevel, material):
    cu = bpy.data.curves.new(name, "CURVE")
    cu.dimensions = "3D"
    cu.bevel_depth = bevel
    cu.bevel_resolution = 4
    sp = cu.splines.new("POLY")
    sp.points.add(len(pts) - 1)
    for p, co, r in zip(sp.points, pts, radii):
        p.co = (*co, 1)
        p.radius = r
    ob = bpy.data.objects.new(name, cu)
    ob.data.materials.append(material)
    scene.collection.objects.link(ob)
    return ob

# Longbow limbs: an arc in the XY plane, thick at the grip and tapering to the tips, tips slightly recurved.
N = 41
R = 4.5
half = math.radians(30)
pts, radii = [], []
for i in range(N):
    t = -1 + 2 * i / (N - 1)
    a = t * half
    x = R * math.sin(a)
    y = R * math.cos(a) - R
    # Tip recurve: bend the last 10% back out.
    k = max(0.0, abs(t) - 0.88) / 0.12
    x += math.copysign(0.08 * k * k, t)
    y -= 0.10 * k * k
    pts.append((x, y, 0))
    radii.append(1.0 - 0.62 * abs(t) ** 1.4)
bow = poly_curve("bow", pts, radii, 0.21, wood)

tipA, tipB = pts[0], pts[-1]
poly_curve("string", [tipA, tipB], [1, 1], 0.04, string)

# Leather grip and horn nocks.
bpy.ops.mesh.primitive_cylinder_add(radius=0.25, depth=0.55, location=(0, 0, 0), rotation=(0, math.radians(90), 0))
grip = bpy.context.object
grip.data.materials.append(leather)
for tip in (tipA, tipB):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.09, location=tip)
    bpy.context.object.data.materials.append(horn)

# Diagonal like the other cards: grip in the middle, limbs from bottom-left to top-right, belly toward the bottom-right.
root = bpy.data.objects.new("root", None)
scene.collection.objects.link(root)
for ob in list(scene.collection.objects):
    if ob is not root:
        ob.parent = root
root.rotation_euler = (math.radians(15), 0, math.radians(45))
bpy.context.view_layer.update()

# Frame every object with a 3 px margin (orthographic camera looking down -Z).
from mathutils import Vector
xs, ys = [], []
for co in pts:
    w = root.matrix_world @ Vector(co)
    xs += [w.x - 0.25, w.x + 0.25]; ys += [w.y - 0.25, w.y + 0.25]
cx, cy = (min(xs) + max(xs)) / 2, (min(ys) + max(ys)) / 2
span = max(max(xs) - min(xs), max(ys) - min(ys)) * SIZE / (SIZE - 6)

cam_data = bpy.data.cameras.new("cam")
cam_data.type = "ORTHO"
cam_data.ortho_scale = span
cam = bpy.data.objects.new("cam", cam_data)
cam.location = (cx, cy, 10)
scene.collection.objects.link(cam)
scene.camera = cam

scene.render.engine = "BLENDER_WORKBENCH"
scene.display.shading.color_type = "MATERIAL"
scene.display.shading.show_specular_highlight = False
scene.display.render_aa = "16"
scene.render.film_transparent = True
scene.render.resolution_x = scene.render.resolution_y = SIZE * SCALE
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.image_settings.color_mode = "RGBA"
scene.view_settings.view_transform = "Standard"

def render(light, path):
    scene.display.shading.light = light
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    big = np.array(bpy.data.images.load(path).pixels[:], dtype=np.float32).reshape(SIZE * SCALE, SIZE * SCALE, 4)
    blocks = big.reshape(SIZE, SCALE, SIZE, SCALE, 4)
    cover = blocks[..., 3].mean((1, 3))
    colour = (blocks[..., :3] * blocks[..., 3:4]).sum((1, 3)) / np.maximum(blocks[..., 3].sum((1, 3))[..., None], 1e-6)
    return colour, cover

flat, cover = render("FLAT", OUT.replace(".png", "-flat.png"))
lit, _ = render("STUDIO", OUT.replace(".png", "-lit.png"))
alpha = cover > 0.4

# Post: the unlit pass says which material each pixel is; the lit pass picks the band of that material's ramp
# (dark, mid, light, highlight) by its brightness rank. Then a 1 px black outline.
def hexes(*hs):
    return np.array([tuple(int(h[i:i + 2], 16) / 255 for i in (1, 3, 5)) for h in hs], dtype=np.float32)
materials = [
    (wood, hexes("#4a2a16", "#7a4626", "#a8693a", "#d09456")),
    (leather, hexes("#2a140c", "#4e2a17", "#76452a")),
    (string, hexes("#9a8f6c", "#d9cfae")),
    (horn, hexes("#a89a74", "#e6dcbc")),
]
flat_srgb = np.array([[c ** (1 / 2.2) for c in m.diffuse_color[:3]] for m, _ in materials], dtype=np.float32)
which = ((flat[..., None, :] - flat_srgb[None, None]) ** 2).sum(-1).argmin(-1)
lum = lit @ np.array([0.299, 0.587, 0.114], dtype=np.float32)
out = np.zeros((SIZE, SIZE, 4), dtype=np.float32)
for k, (_, ramp) in enumerate(materials):
    sel = alpha & (which == k)
    if not sel.any():
        continue
    rank = lum[sel].argsort().argsort() / max(1, sel.sum() - 1)
    band = np.minimum((rank * len(ramp)).astype(int), len(ramp) - 1)
    out[sel, :3] = ramp[band]
    out[sel, 3] = 1
ink = hexes("#141217")[0]
edge = np.zeros_like(alpha)
for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
    edge |= np.roll(np.roll(alpha, dy, 0), dx, 1)
edge &= ~alpha
out[edge, :3] = ink
out[edge, 3] = 1
res = bpy.data.images.new("out", SIZE, SIZE, alpha=True)
res.pixels[:] = out.ravel()
res.filepath_raw = OUT
res.file_format = "PNG"
res.save()
print("SAVED", OUT, int(alpha.sum()), "pixels", [int((alpha & (which == k)).sum()) for k in range(len(materials))])
# 6x preview on the card's dark backdrop, for review.
bg = np.array([0.13, 0.14, 0.16, 1], dtype=np.float32)
prev = np.where(out[..., 3:4] > 0, out, bg).repeat(6, 0).repeat(6, 1)
pv = bpy.data.images.new("preview", SIZE * 6, SIZE * 6, alpha=True)
pv.pixels[:] = prev.ravel()
pv.filepath_raw = OUT.replace(".png", "-x6.png")
pv.file_format = "PNG"
pv.save()
