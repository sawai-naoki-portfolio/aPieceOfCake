"""
Builds every 3D model of "Piece of Cake" and exports one FBX per model for Unity.

Run inside Blender 4.2+ (tested with 5.2):
    path = r"<repo>/ArtSource/build_models.py"
    exec(compile(open(path, encoding="utf-8").read(), path, "exec"), {"__file__": path, "__name__": "__main__"})

Conventions (see Docs/08_ArtPipeline.md):
  * 1 Blender unit = 1 Unity unit = 1 map cell.
  * X is right, Z is up, -Y faces the camera (the model's front).
  * Each model's origin is its bottom centre, except Cell (centre) and parts noted below.
  * Material names are stable: Unity remaps them to project materials of the same name,
    so colours are owned by Unity (Assets/PieceOfCake/Art/Materials), not by this file.
The script works in its own scene ("PieceOfCakeModels") and never touches other scenes.
"""
import math
import os

import bpy

REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT_DIR = os.path.join(REPO, "Assets", "PieceOfCake", "Art", "Models")
BLEND_PATH = os.path.join(REPO, "ArtSource", "PieceOfCakeModels.blend")
SCENE_NAME = "PieceOfCakeModels"

# Preview colours (sRGB). Final colours are set on the Unity materials.
PALETTE = {
    "Mat_Sponge": "#F3C46E", "Mat_Cream": "#FFF8EE", "Mat_PinkCream": "#FFD7DF",
    "Mat_Berry": "#E8364E", "Mat_Leaf": "#4CAF50", "Mat_Chocolate": "#6E3D24",
    "Mat_Jelly": "#73D6BF", "Mat_Ice": "#D6F1FF", "Mat_Plate": "#F4F4F4",
    "Mat_Coat": "#FFFFFF", "Mat_Apron": "#F7A1B5", "Mat_Skin": "#FFDDBF",
    "Mat_Shoe": "#6B4433", "Mat_Eye": "#3B2A22", "Mat_Blush": "#FF9AAA",
    "Mat_Steel": "#C4CDD6", "Mat_Wood": "#B07A4F", "Mat_Ant": "#3A2418",
    "Mat_Fur_Bear": "#B9814F", "Mat_Fur_Rabbit": "#FBF4EE", "Mat_Fur_Cat": "#F4B268",
    "Mat_Fur_Penguin": "#3B4252", "Mat_InnerEar": "#F7B6C6", "Mat_Beak": "#F2A33A",
    "Mat_Dress": "#F7A1C4", "Mat_Hair": "#C98A4B", "Mat_Gold": "#FFD24A",
    "Mat_Candle": "#FFFFFF", "Mat_CandleStripe": "#7CC6F0", "Mat_Flame": "#FFB13B",
    "Mat_SignBoard": "#5E7A4E", "Mat_White": "#FFFFFF", "Mat_Ribbon": "#F06D8C",
}


# --------------------------------------------------------------------------- helpers

def srgb_to_linear(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def material(name):
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name)
        h = PALETTE[name].lstrip("#")
        rgb = [srgb_to_linear(int(h[i:i + 2], 16) / 255) for i in (0, 2, 4)]
        mat.diffuse_color = (*rgb, 1.0)
        nodes = mat.node_tree.nodes if mat.node_tree else None
        if nodes is not None:
            bsdf = next((n for n in nodes if n.type == "BSDF_PRINCIPLED"), None)
            if bsdf is not None:
                bsdf.inputs["Base Color"].default_value = (*rgb, 1.0)
    return mat


def prepare_scene():
    scene = bpy.data.scenes.get(SCENE_NAME) or bpy.data.scenes.new(SCENE_NAME)
    bpy.context.window.scene = scene
    for obj in list(scene.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    for mesh in [m for m in bpy.data.meshes if m.users == 0]:
        bpy.data.meshes.remove(mesh)
    return scene


def finish(obj, mat_name, smooth):
    obj.data.materials.append(material(mat_name))
    if smooth:
        for poly in obj.data.polygons:
            poly.use_smooth = True
    return obj


def cube(name, mat, loc, size, bevel=0.0, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel > 0:
        mod = obj.modifiers.new("Bevel", "BEVEL")
        mod.width = bevel
        mod.segments = 3
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return finish(obj, mat, smooth=bevel > 0)


def sphere(name, mat, loc, size, rot=(0, 0, 0), segments=24, rings=12):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=0.5, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = size
    return finish(obj, mat, smooth=True)


def cylinder(name, mat, loc, radius, depth, rot=(0, 0, 0), vertices=24, smooth=True):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    return finish(obj, mat, smooth=smooth)


def cone(name, mat, loc, r_bottom, r_top, depth, rot=(0, 0, 0), vertices=16):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=r_bottom, radius2=r_top, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.active_object
    obj.name = name
    return finish(obj, mat, smooth=True)


def torus(name, mat, loc, major, minor, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, location=loc, rotation=rot,
                                     major_segments=32, minor_segments=10)
    obj = bpy.context.active_object
    obj.name = name
    return finish(obj, mat, smooth=True)


def prism(name, mat, outline, z0, z1):
    """Vertical prism from a 2D outline [(x, y), ...] (counter-clockwise seen from above)."""
    mesh = bpy.data.meshes.new(name)
    n = len(outline)
    verts = [(x, y, z0) for x, y in outline] + [(x, y, z1) for x, y in outline]
    faces = [list(range(n - 1, -1, -1)), list(range(n, 2 * n))]
    faces += [[i, (i + 1) % n, n + (i + 1) % n, n + i] for i in range(n)]
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    return finish(obj, mat, smooth=False)


def join(parts, name, origin=(0.0, 0.0, 0.0)):
    """Joins parts into one mesh whose origin is at `origin` (world space)."""
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    if len(parts) > 1:
        bpy.ops.object.join()
    obj = bpy.context.active_object
    obj.name = name
    obj.data.name = name
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    bpy.context.scene.cursor.location = origin
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    bpy.context.scene.cursor.location = (0, 0, 0)
    return obj


def group(name, children):
    root = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(root)
    for child in children:
        child.parent = root
    return root


def export(root, file_name):
    bpy.ops.object.select_all(action="DESELECT")
    stack = [root]
    while stack:
        obj = stack.pop()
        obj.select_set(True)
        stack.extend(obj.children)
    os.makedirs(OUT_DIR, exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(OUT_DIR, file_name), use_selection=True, object_types={"MESH", "EMPTY"},
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
        bake_space_transform=True, use_mesh_modifiers=True, mesh_smooth_type="FACE",
        add_leaf_bones=False, use_custom_props=False)
    return root


def park(root, x):
    """Moves an exported model aside so the .blend shows every model in a row."""
    root.location.x += x


# --------------------------------------------------------------------------- cake

def build_cell():
    return join([cube("Cell", "Mat_Sponge", (0, 0, 0), (1, 1, 1), bevel=0.05)], "Cell", origin=(0, 0, 0))


def build_cream_cap():
    parts = [cube("Slab", "Mat_Cream", (0, 0, 0.035), (1.0, 1.0, 0.07), bevel=0.02)]
    for row_y in (-0.38, 0.0, 0.38):
        for i in range(4):
            parts.append(sphere("Dollop", "Mat_Cream", (-0.375 + i * 0.25, row_y, 0.06), (0.26, 0.26, 0.2)))
    return join(parts, "CreamCap")


def strawberry_parts(loc, scale):
    x, y, z = loc
    s = scale
    body = cone("Berry", "Mat_Berry", (x, y, z + 0.35 * s), 0.07 * s, 0.42 * s, 0.7 * s, vertices=20)
    sub = body.modifiers.new("Round", "SUBSURF")
    sub.levels = 2
    bpy.context.view_layer.objects.active = body
    bpy.ops.object.modifier_apply(modifier=sub.name)
    parts = [body]
    for i in range(5):
        a = i * 2 * math.pi / 5
        parts.append(sphere("Leaf", "Mat_Leaf",
                            (x + math.cos(a) * 0.16 * s, y + math.sin(a) * 0.16 * s, z + 0.72 * s),
                            (0.28 * s, 0.1 * s, 0.05 * s), rot=(0, 0, a)))
    parts.append(cylinder("Stem", "Mat_Leaf", (x, y, z + 0.8 * s), 0.03 * s, 0.14 * s))
    return parts


def build_strawberry():
    return join(strawberry_parts((0, 0, 0), 1.0), "Strawberry")


def build_order_slice():
    tip, left, right = (0.0, 0.36), (-0.34, -0.3), (0.34, -0.3)
    outline = [left, right, tip]
    parts = [cylinder("Plate", "Mat_Plate", (0, 0, 0.02), 0.46, 0.04, vertices=32)]
    layers = [("Mat_Sponge", 0.04, 0.2), ("Mat_Cream", 0.2, 0.26), ("Mat_Sponge", 0.26, 0.42), ("Mat_Cream", 0.42, 0.5)]
    for mat, z0, z1 in layers:
        parts.append(prism("Layer", mat, outline, z0, z1))
    parts += strawberry_parts((0.0, -0.02, 0.48), 0.42)
    parts.append(torus("Ribbon", "Mat_Ribbon", (0, 0, 0.06), 0.4, 0.035))
    return join(parts, "OrderSlice")


# --------------------------------------------------------------------------- characters

def build_chef():
    body = join([
        cube("Coat", "Mat_Coat", (0, 0, 0.33), (0.5, 0.4, 0.42), bevel=0.12),
        cube("Apron", "Mat_Apron", (0, -0.2, 0.28), (0.3, 0.02, 0.2), bevel=0.01),
        sphere("Head", "Mat_Skin", (0, 0, 0.68), (0.5, 0.48, 0.48)),
        sphere("EyeL", "Mat_Eye", (-0.08, -0.225, 0.71), (0.07, 0.04, 0.09)),
        sphere("EyeR", "Mat_Eye", (0.08, -0.225, 0.71), (0.07, 0.04, 0.09)),
        sphere("BlushL", "Mat_Blush", (-0.15, -0.19, 0.63), (0.08, 0.03, 0.05)),
        sphere("BlushR", "Mat_Blush", (0.15, -0.19, 0.63), (0.08, 0.03, 0.05)),
        cylinder("HatBand", "Mat_Coat", (0, 0, 0.92), 0.21, 0.12),
        sphere("PuffL", "Mat_Coat", (-0.12, 0, 1.03), (0.27, 0.27, 0.25)),
        sphere("PuffR", "Mat_Coat", (0.12, 0, 1.03), (0.27, 0.27, 0.25)),
        sphere("PuffTop", "Mat_Coat", (0, 0, 1.1), (0.3, 0.3, 0.28)),
    ], "ChefBody")
    foot_l = join([cube("FootL", "Mat_Shoe", (-0.12, -0.02, 0.07), (0.15, 0.24, 0.14), bevel=0.04)], "FootL", origin=(-0.12, 0, 0.07))
    foot_r = join([cube("FootR", "Mat_Shoe", (0.12, -0.02, 0.07), (0.15, 0.24, 0.14), bevel=0.04)], "FootR", origin=(0.12, 0, 0.07))
    # The knife is held in the chef's right hand and points forward (-Y) so that it can swing
    # in the facing direction. Its origin is the hand, the rotation pivot used by Unity.
    pivot = (-0.27, -0.12, 0.36)
    knife = join([
        sphere("Hand", "Mat_Skin", pivot, (0.13, 0.13, 0.13)),
        cylinder("Handle", "Mat_Wood", (pivot[0], pivot[1] - 0.1, pivot[2]), 0.035, 0.16, rot=(math.pi / 2, 0, 0)),
        cube("Blade", "Mat_Steel", (pivot[0], pivot[1] - 0.33, pivot[2]), (0.02, 0.32, 0.09), bevel=0.008),
    ], "Knife", origin=pivot)
    return group("Chef", [body, foot_l, foot_r, knife])


def build_ant():
    parts = [
        sphere("Abdomen", "Mat_Ant", (0.22, 0, 0.2), (0.34, 0.26, 0.28)),
        sphere("Thorax", "Mat_Ant", (0.0, 0, 0.19), (0.22, 0.17, 0.18)),
        sphere("Head", "Mat_Ant", (-0.24, 0, 0.22), (0.26, 0.22, 0.24)),
        sphere("Eye", "Mat_White", (-0.31, -0.09, 0.25), (0.09, 0.05, 0.09)),
        sphere("Pupil", "Mat_Eye", (-0.33, -0.115, 0.25), (0.045, 0.02, 0.045)),
    ]
    for i, lx in enumerate((-0.09, 0.0, 0.09)):
        for side in (-1, 1):
            tilt = (0.35 if side < 0 else -0.35)
            parts.append(cylinder("Leg", "Mat_Ant", (lx + (i - 1) * 0.04, side * 0.1, 0.09), 0.018, 0.22,
                                  rot=(tilt, (i - 1) * 0.4, 0), vertices=8))
    for side in (-1, 1):
        parts.append(cylinder("Antenna", "Mat_Ant", (-0.36, side * 0.05, 0.36), 0.014, 0.2,
                              rot=(side * 0.3, -0.6, 0), vertices=8))
    return join(parts, "Ant")


def customer_base(fur, body_mat=None):
    body_mat = body_mat or fur
    return [
        sphere("Body", body_mat, (0, 0.05, 0.55), (0.9, 0.8, 1.1)),
        sphere("Head", fur, (0, 0.0, 1.38), (0.72, 0.68, 0.68)),
        sphere("EyeL", "Mat_Eye", (-0.12, -0.32, 1.42), (0.08, 0.04, 0.1)),
        sphere("EyeR", "Mat_Eye", (0.12, -0.32, 1.42), (0.08, 0.04, 0.1)),
        sphere("BlushL", "Mat_Blush", (-0.21, -0.28, 1.31), (0.11, 0.04, 0.07)),
        sphere("BlushR", "Mat_Blush", (0.21, -0.28, 1.31), (0.11, 0.04, 0.07)),
    ]


def build_customer(kind):
    if kind == "Bear":
        parts = customer_base("Mat_Fur_Bear")
        parts += [sphere("EarL", "Mat_Fur_Bear", (-0.24, 0.0, 1.66), (0.22, 0.14, 0.22)),
                  sphere("EarR", "Mat_Fur_Bear", (0.24, 0.0, 1.66), (0.22, 0.14, 0.22)),
                  sphere("Muzzle", "Mat_Cream", (0, -0.3, 1.3), (0.24, 0.12, 0.17))]
    elif kind == "Rabbit":
        parts = customer_base("Mat_Fur_Rabbit")
        for side in (-1, 1):
            parts += [sphere("Ear", "Mat_Fur_Rabbit", (side * 0.12, 0.0, 1.85), (0.17, 0.12, 0.55), rot=(0, side * 0.15, 0)),
                      sphere("Inner", "Mat_InnerEar", (side * 0.12, -0.05, 1.85), (0.08, 0.05, 0.4), rot=(0, side * 0.15, 0))]
    elif kind == "Cat":
        parts = customer_base("Mat_Fur_Cat")
        for side in (-1, 1):
            parts.append(cone("Ear", "Mat_Fur_Cat", (side * 0.2, 0.0, 1.7), 0.12, 0.0, 0.24, rot=(0, side * 0.35, 0), vertices=4))
    elif kind == "Penguin":
        parts = customer_base("Mat_Fur_Penguin")
        parts += [sphere("Belly", "Mat_White", (0, -0.22, 0.5), (0.6, 0.4, 0.85)),
                  sphere("Face", "Mat_White", (0, -0.2, 1.33), (0.5, 0.35, 0.42)),
                  cone("Beak", "Mat_Beak", (0, -0.38, 1.3), 0.06, 0.0, 0.14, rot=(math.pi / 2, 0, 0), vertices=8)]
    elif kind == "Princess":
        parts = customer_base("Mat_Skin", body_mat="Mat_Dress")
        parts += [sphere("Hair", "Mat_Hair", (0, 0.05, 1.47), (0.76, 0.72, 0.6)),
                  cylinder("Crown", "Mat_Gold", (0, 0.0, 1.78), 0.17, 0.1)]
        for i in range(5):
            a = i * 2 * math.pi / 5
            parts.append(cone("Spike", "Mat_Gold", (math.cos(a) * 0.15, math.sin(a) * 0.15, 1.88), 0.045, 0.0, 0.12, vertices=6))
    else:
        raise ValueError(kind)
    return join(parts, "Customer_" + kind)


# --------------------------------------------------------------------------- props

def build_table():
    return join([
        cylinder("Foot", "Mat_Wood", (0, 0, 0.02), 0.26, 0.04),
        cylinder("Leg", "Mat_Wood", (0, 0, 0.25), 0.06, 0.46),
        cylinder("Top", "Mat_White", (0, 0, 0.5), 0.62, 0.06, vertices=32),
        cylinder("Plate", "Mat_Plate", (0, 0, 0.545), 0.32, 0.03, vertices=32),
    ], "Table")


def build_candle():
    body = [cylinder("Plate", "Mat_Plate", (0, 0, 0.02), 0.26, 0.04),
            cylinder("Wax", "Mat_Candle", (0, 0, 0.39), 0.09, 0.7)]
    for i in range(4):
        body.append(torus("Stripe", "Mat_CandleStripe", (0, 0, 0.15 + i * 0.16), 0.09, 0.018, rot=(0.25, 0, 0)))
    body.append(cylinder("Wick", "Mat_Eye", (0, 0, 0.78), 0.012, 0.08, vertices=8))
    candle = join(body, "CandleBody")
    flame = join([sphere("Flame", "Mat_Flame", (0, 0, 0.9), (0.14, 0.14, 0.26))], "Flame", origin=(0, 0, 0.82))
    return group("Candle", [candle, flame])


def build_fork():
    parts = [cube("Bar", "Mat_Steel", (0, 0, 0.09), (0.9, 0.16, 0.18), bevel=0.04)]
    for x in (-0.3, 0.0, 0.3):
        parts.append(cube("Tine", "Mat_Steel", (x, 0, 0.42), (0.11, 0.1, 0.5), bevel=0.02))
        parts.append(cone("Tip", "Mat_Steel", (x, 0, 0.74), 0.07, 0.0, 0.16, vertices=8))
    return join(parts, "Fork")


def build_sign():
    return join([
        cylinder("Post", "Mat_Wood", (0, 0.02, 0.3), 0.05, 0.6, vertices=12),
        cube("Frame", "Mat_Wood", (0, 0.02, 0.75), (0.8, 0.08, 0.52), bevel=0.04),
        cube("Board", "Mat_SignBoard", (0, -0.03, 0.75), (0.68, 0.04, 0.4), bevel=0.02),
        cube("Bang", "Mat_White", (0, -0.06, 0.8), (0.07, 0.02, 0.2), bevel=0.01),
        sphere("Dot", "Mat_White", (0, -0.06, 0.63), (0.08, 0.03, 0.08)),
    ], "Sign")


# --------------------------------------------------------------------------- main

BUILDERS = [
    ("Cell.fbx", build_cell),
    ("CreamCap.fbx", build_cream_cap),
    ("Strawberry.fbx", build_strawberry),
    ("OrderSlice.fbx", build_order_slice),
    ("Chef.fbx", build_chef),
    ("Ant.fbx", build_ant),
    ("Customer_Bear.fbx", lambda: build_customer("Bear")),
    ("Customer_Rabbit.fbx", lambda: build_customer("Rabbit")),
    ("Customer_Cat.fbx", lambda: build_customer("Cat")),
    ("Customer_Penguin.fbx", lambda: build_customer("Penguin")),
    ("Customer_Princess.fbx", lambda: build_customer("Princess")),
    ("Table.fbx", build_table),
    ("Candle.fbx", build_candle),
    ("Fork.fbx", build_fork),
    ("Sign.fbx", build_sign),
]


def main():
    prepare_scene()
    exported = []
    for i, (file_name, builder) in enumerate(BUILDERS):
        root = builder()
        export(root, file_name)
        park(root, i * 2.5)
        exported.append(file_name)
    bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH, copy=True)
    print("Exported", len(exported), "models to", OUT_DIR)
    print("\n".join(exported))


if __name__ == "__main__":
    main()
