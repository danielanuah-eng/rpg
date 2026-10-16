using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonBDX
{
    /// <summary>
    /// Presentation only. Reads imported Arena sub-sprites, never changes unit/domain position.
    /// Attach to the unit root and pass its separate sprite/model child as visual.
    /// </summary>
    public sealed class DungeonActorAnimation : MonoBehaviour
    {
        static readonly string[] Directions = {
            "south", "south-east", "east", "north-east", "north", "north-west", "west", "south-west"
        };
        static readonly Dictionary<string, FrameLibrary> Cache = new Dictionary<string, FrameLibrary>(StringComparer.Ordinal);
        static readonly string[] IdleClips = { "idle" };
        static readonly string[] WalkClips = { "walk", "run", "move" };
        static readonly string[] AttackClips = { "attack", "cast", "zap", "summon" };
        static readonly string[] CastClips = { "cast", "zap", "summon", "attack" };
        static readonly string[] HurtClips = { "hit", "hurt", "damage" };
        static readonly string[] DeathClips = { "death", "defeat", "die" };

        FrameLibrary library;
        Transform visual;
        SpriteRenderer spriteRenderer;
        Sprite fallback;
        Camera viewCamera;
        Vector3 basePosition, baseScale, worldDirection;
        Quaternion baseRotation;
        string state = "idle", clip;
        float elapsed, proceduralTime, actionDuration;
        bool moving, dead, action;
        int direction;

        public string State => state;
        public bool DeathComplete => dead && elapsed >= actionDuration;
        public string FacingDirection => Directions[direction];
        public Sprite CurrentSprite => spriteRenderer ? spriteRenderer.sprite : null;
        public int AnimationFrameCount
        {
            get { var frames = Frames(out _); return frames == null ? (CurrentSprite ? 1 : 0) : frames.Length; }
        }

        public void Initialize(string spriteFolder, Sprite fallbackSprite, Transform visualChild, Camera camera)
        {
            visual = visualChild;
            fallback = fallbackSprite;
            viewCamera = camera;
            spriteRenderer = visual ? visual.GetComponentInChildren<SpriteRenderer>(true) : null;
            if (!fallback && spriteRenderer) fallback = spriteRenderer.sprite;
            library = spriteRenderer ? Load(spriteFolder) : null;
            if (visual)
            {
                basePosition = visual.localPosition;
                baseScale = visual.localScale;
                baseRotation = visual.localRotation;
            }
            var forward = camera ? camera.transform.forward : Vector3.forward;
            forward.y = 0;
            worldDirection = forward.sqrMagnitude > .0001f ? -forward.normalized : Vector3.back;
            moving = dead = action = false;
            elapsed = proceduralTime = 0;
            state = "idle";
            ChooseClip();
            RefreshFacing();
        }

        public void SetMoving(Vector3 directionInWorld, bool isMoving)
        {
            if (dead) return;
            UpdateWorldDirection(directionInWorld);
            moving = isMoving;
            if (!action)
            {
                string next = moving ? "walk" : "idle";
                if (state != next) { state = next; elapsed = 0; ChooseClip(); }
            }
            RefreshFacing();
        }

        public void PlayAction(Vector3 directionInWorld, bool hurt = false, bool dead = false, bool cast = false)
        {
            if (this.dead) return;
            // A hit reaction must not hide the actor's own paid attack/cast feedback.
            if (hurt && action && (state == "attack" || state == "cast") && !dead) return;
            UpdateWorldDirection(directionInWorld);
            this.dead = dead;
            if (dead) moving = false;
            state = dead ? "death" : hurt ? "hurt" : cast ? "cast" : "attack";
            elapsed = 0;
            action = true;
            ChooseClip();
            RefreshFacing();
        }

        public void Revive()
        {
            moving = dead = action = false;
            elapsed = 0;
            state = "idle";
            ChooseClip();
            RefreshFacing();
        }

        void Update() => Advance(Time.deltaTime);

        /// <summary>Advances visual time; public for deterministic presentation tests.</summary>
        public void Advance(float deltaTime)
        {
            if (!visual || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0) return;
            elapsed += deltaTime;
            proceduralTime += deltaTime;
            if (action && !dead && elapsed >= actionDuration)
            {
                elapsed = Mathf.Max(0, elapsed - actionDuration);
                action = false;
                state = moving ? "walk" : "idle";
                ChooseClip();
            }
            RefreshFacing();
        }

        public void RefreshFacing()
        {
            if (!visual) return;
            direction = RelativeDirection(worldDirection, viewCamera);
            if (spriteRenderer)
            {
                var frames = Frames(out bool flipped);
                Sprite selected = null;
                if (frames != null && frames.Length > 0)
                {
                    int index = Mathf.FloorToInt(elapsed * Fps(clip));
                    index = action ? Mathf.Min(index, frames.Length - 1) : index % frames.Length;
                    selected = frames[Mathf.Max(0, index)];
                }
                spriteRenderer.sprite = selected ? selected : Still();
                spriteRenderer.flipX = selected && flipped;
                if (viewCamera) visual.rotation = viewCamera.transform.rotation;
                // Recompute after EVERY frame and camera orbit; pivots/frame bounds may differ.
                if (visual != transform && spriteRenderer.sprite)
                {
                    Bounds bounds = spriteRenderer.sprite.bounds;
                    Vector3 scale = visual.lossyScale;
                    float center = bounds.center.x * (spriteRenderer.flipX ? -1 : 1);
                    visual.position = transform.position + Vector3.up * .06f
                        - visual.right * (center * Mathf.Abs(scale.x))
                        - visual.up * (bounds.min.y * Mathf.Abs(scale.y));
                }
            }
            else ApplyModelFeedback();
        }

        void ApplyModelFeedback()
        {
            // Original native FBX have no skeletal clips. This is deliberately transform feedback.
            float phase = Mathf.Clamp01(elapsed / Mathf.Max(.01f, actionDuration));
            float pulse = action ? Mathf.Sin(Mathf.PI * phase) : 0;
            float bob = dead ? 0 : moving ? Mathf.Abs(Mathf.Sin(proceduralTime * 11)) * .035f
                                                        : Mathf.Sin(proceduralTime * 2.4f) * .009f;
            Vector3 flat = worldDirection; flat.y = 0;
            if (flat.sqrMagnitude < .0001f) flat = Vector3.forward;
            flat.Normalize();
            if (visual != transform)
                visual.localPosition = basePosition + Vector3.up * bob
                    + transform.InverseTransformDirection(flat) * (state == "attack" || state == "cast" ? pulse * .09f : 0);
            float tilt = dead ? 75 : state == "hurt" ? -pulse * 9 : (state == "attack" || state == "cast") ? pulse * 10 : 0;
            visual.rotation = Quaternion.LookRotation(flat, Vector3.up) * baseRotation * Quaternion.Euler(tilt, 0, 0);
            visual.localScale = dead ? Vector3.Scale(baseScale, new Vector3(1, .65f, 1)) : baseScale;
        }

        void UpdateWorldDirection(Vector3 candidate)
        {
            candidate.y = 0;
            if (!Finite(candidate.x) || !Finite(candidate.z) || candidate.sqrMagnitude < .0001f) return;
            worldDirection = candidate.normalized;
        }

        void ChooseClip()
        {
            string[] candidates = state == "walk" ? WalkClips : state == "attack" ? AttackClips
                : state == "cast" ? CastClips : state == "hurt" ? HurtClips : state == "death" ? DeathClips : IdleClips;
            clip = null;
            if (library != null)
                foreach (string candidate in candidates)
                    if (library.Clips.ContainsKey(candidate)) { clip = candidate; break; }
            int longest = 0;
            if (clip != null)
                foreach (var frames in library.Clips[clip])
                    if (frames != null) longest = Mathf.Max(longest, frames.Length);
            actionDuration = longest > 0 ? longest / Fps(clip)
                : state == "hurt" ? .3f : state == "death" ? .8f : .65f;
        }

        Sprite[] Frames(out bool flip)
        {
            flip = false;
            if (library == null || clip == null || !library.Clips.TryGetValue(clip, out var byDirection)) return null;
            if (Has(byDirection[direction])) return byDirection[direction];
            int mirror = (8 - direction) % 8;
            if (Has(byDirection[mirror])) { flip = true; return byDirection[mirror]; }
            for (int distance = 1; distance <= 4; distance++)
            {
                int right = (direction + distance) % 8;
                int left = (direction + 8 - distance) % 8;
                if (Has(byDirection[right])) return byDirection[right];
                if (Has(byDirection[left])) return byDirection[left];
            }
            return null;
        }

        Sprite Still()
        {
            if (library != null)
            {
                if (library.Still[direction]) return library.Still[direction];
                for (int distance = 1; distance <= 4; distance++)
                {
                    if (library.Still[(direction + distance) % 8]) return library.Still[(direction + distance) % 8];
                    if (library.Still[(direction + 8 - distance) % 8]) return library.Still[(direction + 8 - distance) % 8];
                }
            }
            return fallback;
        }

        static FrameLibrary Load(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) return null;
            folder = folder.Trim().Trim('/');
            if (folder.StartsWith("Sprites/", StringComparison.Ordinal)) folder = folder.Substring(8);
            if (Cache.TryGetValue(folder, out var cached)) return cached;
            var result = new FrameLibrary();
            foreach (var sprite in Resources.LoadAll<Sprite>("Sprites/" + folder))
            {
                int stillDirection = DirectionIndex(sprite.name);
                if (stillDirection >= 0) { result.Still[stillDirection] = sprite; continue; }
                int last = sprite.name.LastIndexOf('_');
                int previous = last > 0 ? sprite.name.LastIndexOf('_', last - 1) : -1;
                if (previous <= 0 || !int.TryParse(sprite.name.Substring(last + 1), out int index)
                    || index < 0 || index > 4096) continue;
                int d = DirectionIndex(sprite.name.Substring(previous + 1, last - previous - 1));
                if (d < 0) continue;
                string name = sprite.name.Substring(0, previous).ToLowerInvariant();
                if (!result.Clips.TryGetValue(name, out var directions))
                    result.Clips[name] = directions = new Sprite[8][];
                Sprite[] frames = directions[d] ?? Array.Empty<Sprite>();
                if (frames.Length <= index) Array.Resize(ref frames, index + 1);
                frames[index] = sprite;
                directions[d] = frames;
            }
            Cache[folder] = result;
            return result;
        }

        static int DirectionIndex(string name)
        {
            string key = name.Replace("-", "").ToLowerInvariant();
            for (int i = 0; i < Directions.Length; i++)
                if (Directions[i].Replace("-", "") == key) return i;
            return -1;
        }

        static int RelativeDirection(Vector3 world, Camera camera)
        {
            Vector3 north = camera ? camera.transform.forward : Vector3.forward;
            north.y = 0;
            if (north.sqrMagnitude < .0001f && camera) { north = camera.transform.up; north.y = 0; }
            if (north.sqrMagnitude < .0001f) north = Vector3.forward;
            north.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, north);
            float angle = Mathf.Atan2(Vector3.Dot(world, right), Vector3.Dot(world, -north)) * Mathf.Rad2Deg;
            return (Mathf.RoundToInt(angle / 45) % 8 + 8) % 8;
        }

        static float Fps(string name) => name == "idle" ? 5 : name == "walk" || name == "run" || name == "move" ? 10
            : name == "hit" || name == "hurt" || name == "damage" ? 8 : name == "death" || name == "die" || name == "defeat" ? 6 : 7;
        static bool Has(Sprite[] frames) => frames != null && frames.Length > 0;
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        sealed class FrameLibrary
        {
            public readonly Sprite[] Still = new Sprite[8];
            public readonly Dictionary<string, Sprite[][]> Clips = new Dictionary<string, Sprite[][]>(StringComparer.Ordinal);
        }
    }
}
