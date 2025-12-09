using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MapleFighters.Tests.PlayMode
{
    /// <summary>
    /// Example PlayMode tests that run in Play mode and can test Unity-specific behavior.
    /// </summary>
    [TestFixture]
    public class GameObjectTests
    {
        private GameObject testObject;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestObject");
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.Destroy(testObject);
            }
        }

        [UnityTest]
        public IEnumerator GameObject_WhenCreated_IsActive()
        {
            // Assert
            Assert.IsTrue(testObject.activeSelf);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameObject_WhenDisabled_IsNotActive()
        {
            // Act
            testObject.SetActive(false);
            yield return null;

            // Assert
            Assert.IsFalse(testObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator Rigidbody2D_WhenAdded_HasDefaultSettings()
        {
            // Arrange
            var rb = testObject.AddComponent<Rigidbody2D>();
            yield return null;

            // Assert
            Assert.IsNotNull(rb);
            Assert.AreEqual(1f, rb.mass);
        }

        [UnityTest]
        public IEnumerator Transform_WhenMoved_UpdatesPosition()
        {
            // Arrange
            var initialPosition = testObject.transform.position;
            var targetPosition = new Vector3(10, 5, 0);

            // Act
            testObject.transform.position = targetPosition;
            yield return null;

            // Assert
            Assert.AreEqual(targetPosition, testObject.transform.position);
            Assert.AreNotEqual(initialPosition, testObject.transform.position);
        }

        [UnityTest]
        public IEnumerator Rigidbody2D_WhenVelocitySet_MovesObject()
        {
            // Arrange
            var rb = testObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // Disable gravity for predictable test
            var initialPosition = testObject.transform.position;

            // Act
            rb.velocity = new Vector2(10, 0);
            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.Greater(testObject.transform.position.x, initialPosition.x);
        }
    }
}
