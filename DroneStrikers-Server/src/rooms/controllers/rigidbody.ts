import { TransformState } from "@rooms/schema/TransformState";
import { Vector2 } from "src/types/commonTypes";
import { Constants } from "src/utils";

export interface RigidbodyProperties {
  mass: number;
  drag: number;
  elasticity: number; // Bounciness factor (0 = no bounce, 1 = full bounce)
  isKinematic: boolean; // If true, behaves like infinite mass and is not moved by collisions
}

export class Rigidbody {
  private parentTransform: TransformState;
  private properties: RigidbodyProperties;

  private accumulatedForce: Vector2 = { x: 0, y: 0 };

  constructor(parentTransform: TransformState, properties: Partial<RigidbodyProperties>) {
    this.parentTransform = parentTransform;
    this.properties = {
      mass: properties.mass ?? 1,
      drag: properties.drag ?? 0,
      elasticity: properties.elasticity ?? 0,
      isKinematic: properties.isKinematic ?? false,
    };
  }

  public get isKinematic(): boolean {
    return !!this.properties.isKinematic;
  }

  /**
   * Returns the elasticity (bounciness) of the rigidbody.
   */
  public get elasticity(): number {
    return this.properties.elasticity;
  }

  public getInverseMass(): number {
    const m = this.properties.mass;
    return this.isKinematic || m <= 0 ? 0 : 1 / m;
  }

  /**
   * Translate the rigidbody by the given delta values.
   * @param dx Delta x value
   * @param dy Delta y value
   */
  public translate(dx: number, dy: number) {
    this.parentTransform.posX += dx;
    this.parentTransform.posY += dy;
  }

  /**
   * Physics step update (called once per frame).
   * @param deltaTime Time elapsed since last update (in seconds). Defaults to fixed time step.
   */
  public update(deltaTime: number = Constants.FIXED_TIME_STEP_S) {
    if (this.isKinematic || this.properties.mass <= 0) {
      // Kinematic/infinite-mass objects aren't integrated by forces
      this.accumulatedForce = { x: 0, y: 0 };
      this.parentTransform.velX = 0;
      this.parentTransform.velY = 0;
      return;
    }

    // Convert accumulated force to acceleration
    const invMass = this.getInverseMass();
    const ax = this.accumulatedForce.x * invMass;
    const ay = this.accumulatedForce.y * invMass;

    // Integrate velocity
    this.parentTransform.velX += ax * deltaTime;
    this.parentTransform.velY += ay * deltaTime;

    // Linear drag as force: Fd = -drag * v
    if (this.properties.drag > 0) {
      const dragCoeff = this.properties.drag;
      this.parentTransform.velX += -dragCoeff * this.parentTransform.velX * deltaTime;
      this.parentTransform.velY += -dragCoeff * this.parentTransform.velY * deltaTime;
    }

    // Integrate position
    this.parentTransform.posX += this.parentTransform.velX * deltaTime;
    this.parentTransform.posY += this.parentTransform.velY * deltaTime;

    // Clear forces for next frame
    this.accumulatedForce.x = 0;
    this.accumulatedForce.y = 0;
  }

  /**
   * Apply a continuous force (will be integrated this frame).
   * Force scales by 1/m internally when converted to acceleration.
   */
  public applyForce(force: Vector2) {
    this.accumulatedForce.x += force.x;
    this.accumulatedForce.y += force.y;
  }

  /**
   * Apply an instantaneous impulse force. Directly modifies velocity.
   */
  public applyImpulse(impulse: Vector2) {
    const invMass = 1 / this.properties.mass;
    this.parentTransform.velX += impulse.x * invMass;
    this.parentTransform.velY += impulse.y * invMass;
  }

  /**
   * Directly set the velocity of the rigidbody.
   * @param v The velocity to set.
   */
  public setVelocity(v: Vector2) {
    this.parentTransform.velX = v.x;
    this.parentTransform.velY = v.y;
  }
}
