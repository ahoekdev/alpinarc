import { Inject, Injectable } from '@nestjs/common';
import { type Database, DATABASE } from '../database/database.provider.js';
import { lodges } from '../database/schema.js';

@Injectable()
export class LodgesService {
  constructor(@Inject(DATABASE) private readonly db: Database) {}

  async findAll() {
    return this.db.select().from(lodges);
  }
}
