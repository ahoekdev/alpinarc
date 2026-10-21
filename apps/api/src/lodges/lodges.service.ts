import { Inject, Injectable, NotFoundException } from '@nestjs/common';
import { eq } from 'drizzle-orm';
import { type Database, DATABASE } from '../database/database.provider.js';
import { lodges } from '../database/schema.js';
import type { CreateLodgeDto } from './lodges.schema.js';

@Injectable()
export class LodgesService {
  constructor(@Inject(DATABASE) private readonly db: Database) {}

  async findAll() {
    return this.db.select().from(lodges);
  }

  async findOne(id: string) {
    const [lodge] = await this.db
      .select()
      .from(lodges)
      .where(eq(lodges.id, id));

    if (!lodge) {
      throw new NotFoundException(`Lodge with id ${id} not found`);
    }

    return lodge;
  }

  async create(createLodgeDto: CreateLodgeDto) {
    const [lodge] = await this.db
      .insert(lodges)
      .values(createLodgeDto)
      .returning();

    return lodge;
  }
}
