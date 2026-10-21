import { pgTable, uuid, varchar } from 'drizzle-orm/pg-core';

export const lodges = pgTable('lodges', {
  id: uuid().primaryKey().defaultRandom(),
  name: varchar({ length: 255 }).notNull(),
});

export type Lodge = typeof lodges.$inferSelect;
export type NewLodge = typeof lodges.$inferInsert;
