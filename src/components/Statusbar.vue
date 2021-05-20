<template>
  <div id='statusbar'>
    <p id='status-bar' v-if="shouldDisplay" class='form-control'> Last updated: {{new Date(Date.parse(this.timeOfRequest)).toLocaleString("en-GB")}} </p>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator'

@Component
export default class Statusbar extends Vue {

  @Prop()
  timeOfRequest!: string
  
  @Prop()
  shouldDisplay!: boolean
  
}

</script>

<style scoped>
#status-bar {
  text-align: center;
  width: 320px;
  position: absolute;
  bottom: 0;
  left: 0;
  margin-left: 20px;
  margin-bottom: 20px;
}
</style>
