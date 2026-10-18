import bpy, sys, numpy as np
a, b, out = sys.argv[sys.argv.index("--") + 1:]
bg = np.array([0.13, 0.14, 0.16, 1], dtype=np.float32)
def load(p):
    im = bpy.data.images.load(p); px = np.array(im.pixels[:], dtype=np.float32).reshape(im.size[1], im.size[0], 4)
    return np.where(px[..., 3:4] > 0.5, px, bg).repeat(6, 0).repeat(6, 1)
A, Bm = load(a), load(b)
gap = np.tile(bg, (A.shape[0], 24, 1))
img = np.concatenate([A, gap, Bm], 1)
res = bpy.data.images.new("cmp", img.shape[1], img.shape[0], alpha=True)
res.pixels[:] = img.ravel(); res.filepath_raw = out; res.file_format = "PNG"; res.save()
