using NUnit.Framework;
using UnityEngine;

namespace DungeonBDX.Tests
{
    public sealed class DungeonActorAnimationTests
    {
        GameObject root, visual, cameraObject;
        Camera camera;
        DungeonActorAnimation animation;
        Texture2D texture;
        Sprite fallback;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Actor animation test");
            root.transform.position = new Vector3(3, 0, 4);
            visual = new GameObject("Sprite visual", typeof(SpriteRenderer));
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * .4f;
            cameraObject = new GameObject("Animation camera", typeof(Camera));
            camera = cameraObject.GetComponent<Camera>();
            camera.transform.position = root.transform.position + new Vector3(0, 8, -8);
            camera.transform.LookAt(root.transform.position);
            animation = root.AddComponent<DungeonActorAnimation>();
            texture = new Texture2D(32, 48);
            fallback = Sprite.Create(texture, new Rect(0, 0, 32, 48), new Vector2(.2f, .8f), 32);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(fallback);
            Object.DestroyImmediate(texture);
        }

        [Test]
        public void RealImportedAtlasHasIdleAndWalkFramesThatAdvance()
        {
            animation.Initialize("Alvyr", fallback, visual.transform, camera);
            Assert.That(animation.AnimationFrameCount, Is.GreaterThan(1), "Alvyr idle sub-sprites must be imported.");
            Sprite first = animation.CurrentSprite;
            animation.Advance(.21f);
            Assert.That(animation.CurrentSprite, Is.Not.SameAs(first));
            animation.SetMoving(Vector3.right, true);
            Assert.That(animation.State, Is.EqualTo("walk"));
            Assert.That(animation.AnimationFrameCount, Is.GreaterThan(1));
            first = animation.CurrentSprite;
            animation.Advance(.11f);
            Assert.That(animation.CurrentSprite, Is.Not.SameAs(first));
        }

        [Test]
        public void AttackAdvancesAndReturnsToIdleWithoutRootMotion()
        {
            animation.Initialize("Alvyr", fallback, visual.transform, camera);
            Vector3 position = root.transform.position;
            animation.PlayAction(Vector3.right);
            Assert.That(animation.State, Is.EqualTo("attack"));
            Assert.That(animation.AnimationFrameCount, Is.GreaterThan(1));
            Sprite first = animation.CurrentSprite;
            animation.Advance(.15f);
            Assert.That(animation.CurrentSprite, Is.Not.SameAs(first));
            animation.Advance(20);
            Assert.That(animation.State, Is.EqualTo("idle"));
            Assert.That(root.transform.position, Is.EqualTo(position));
        }

        [Test]
        public void MovingActorResumesWalkAfterCastAndHitDoesNotCutCast()
        {
            animation.Initialize("Alvyr", fallback, visual.transform, camera);
            animation.SetMoving(Vector3.forward, true);
            animation.PlayAction(Vector3.right, cast: true);
            Assert.That(animation.State, Is.EqualTo("cast"));
            animation.PlayAction(Vector3.left, hurt: true);
            Assert.That(animation.State, Is.EqualTo("cast"));
            animation.Advance(20);
            Assert.That(animation.State, Is.EqualTo("walk"));
        }

        [Test]
        public void DeathHoldsFinalFrameAndIgnoresFurtherMovement()
        {
            animation.Initialize("Alvyr", fallback, visual.transform, camera);
            animation.PlayAction(Vector3.forward, dead: true);
            animation.Advance(20);
            Sprite corpse = animation.CurrentSprite;
            animation.SetMoving(Vector3.right, true);
            animation.PlayAction(Vector3.right);
            animation.Advance(2);
            Assert.That(animation.State, Is.EqualTo("death"));
            Assert.That(animation.CurrentSprite, Is.SameAs(corpse));
        }

        [Test]
        public void ReusedCompanionCanAnimateAgainAfterDeath()
        {
            animation.Initialize("Alvyr", fallback, visual.transform, camera);
            animation.PlayAction(Vector3.forward, dead: true);
            animation.Advance(20);
            animation.Revive();
            Assert.That(animation.State, Is.EqualTo("idle"));
            animation.PlayAction(Vector3.right);
            Assert.That(animation.State, Is.EqualTo("attack"));
            Sprite first = animation.CurrentSprite;
            animation.Advance(.15f);
            Assert.That(animation.CurrentSprite, Is.Not.SameAs(first));
        }

        [Test]
        public void MissingFolderKeepsFallbackAndDoesNotThrowOnActions()
        {
            animation.Initialize("NoSuchDungeonActorFolder", fallback, visual.transform, camera);
            animation.SetMoving(Vector3.left, true);
            animation.PlayAction(Vector3.left);
            animation.Advance(20);
            Assert.That(animation.CurrentSprite, Is.SameAs(fallback));
            Assert.That(animation.AnimationFrameCount, Is.EqualTo(1));
            Assert.That(animation.State, Is.EqualTo("walk"));
        }

        [Test]
        public void CameraOrbitChangesRelativeDirectionAndKeepsFeetOnRoot()
        {
            animation.Initialize(null, fallback, visual.transform, camera);
            animation.SetMoving(Vector3.forward, false);
            string before = animation.FacingDirection;
            AssertFeetAnchored();
            camera.transform.position = root.transform.position + new Vector3(8, 8, 0);
            camera.transform.LookAt(root.transform.position);
            animation.RefreshFacing();
            Assert.That(animation.FacingDirection, Is.Not.EqualTo(before));
            Assert.That(Quaternion.Angle(visual.transform.rotation, camera.transform.rotation), Is.LessThan(.001f));
            AssertFeetAnchored();
        }

        [Test]
        public void NativeModelGetsFeedbackWithoutMovingActorRoot()
        {
            Object.DestroyImmediate(visual.GetComponent<SpriteRenderer>());
            animation.Initialize(null, null, visual.transform, camera);
            Vector3 position = root.transform.position;
            animation.SetMoving(Vector3.right, true);
            animation.Advance(.1f);
            Assert.That(visual.transform.localPosition.y, Is.GreaterThan(0));
            animation.PlayAction(Vector3.forward);
            animation.Advance(.2f);
            Assert.That(visual.transform.localPosition.z, Is.GreaterThan(0));
            animation.Advance(10);
            Assert.That(animation.State, Is.EqualTo("walk"));
            Assert.That(root.transform.position, Is.EqualTo(position));
        }

        [Test]
        public void UprightKeepsPivotFeetOnTheCell()
        {
            animation.Upright = true;
            animation.Initialize("Alvyr", fallback, visual.transform, camera);
            Vector3 pivot = visual.transform.position;
            Assert.That(Mathf.Abs(pivot.y - (root.transform.position.y + .03f)), Is.LessThan(.0001f));
            Assert.That(Vector2.Distance(new Vector2(pivot.x, pivot.z), new Vector2(root.transform.position.x, root.transform.position.z)),
                Is.LessThan(.05f));
            Assert.That(Mathf.Abs(visual.transform.eulerAngles.x), Is.LessThan(.001f));
        }

        void AssertFeetAnchored()
        {
            Bounds bounds = animation.CurrentSprite.bounds;
            Vector3 feet = visual.transform.TransformPoint(new Vector3(bounds.center.x, bounds.min.y, 0));
            Assert.That(Vector3.Distance(feet, root.transform.position + Vector3.up * .06f), Is.LessThan(.0001f));
        }
    }
}
