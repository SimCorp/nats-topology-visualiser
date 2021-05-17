<template>
  <div id='statusbar'>
    <p id='status-bar' v-if="shouldDisplay" class='form-control'> Last updated: {{new Date(Date.parse(this.timeOfRequest)).toLocaleString("en-GB")}} </p>
  </div>
</template>

<script lang="ts">

export default {
  name: "Statusbar",
  props: {
    timeOfRequest: String,
    shouldDisplay: Boolean
  }
}

</script>

<style scoped>
#status-bar {
  text-align: center;
  width: 360px;
  position: absolute;
  bottom: 0;
  left: 0;
  margin-left: 50px;
  margin-bottom: 20px;
}
</style>
